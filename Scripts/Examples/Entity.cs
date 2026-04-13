using System;
using System.Collections.Generic;
using System.Linq;
using TDP.InteractiveComponents.Presets;

#nullable enable
namespace TDP.InteractiveComponents.Examples
{
    public class Entity : LoadedModule
    {
        public string displayName { get; protected set; } = "[entity]";

        public virtual void SetDisplayName(string name)
        {
            displayName = name;
        }

        public virtual void Die()
        {
            MessageOutput.Log("死");

            Decommission();
        }
    }

    public class EntityHealth : LoadedModule
    {
        public EntityHealth()
        {
            // create health attribute
            AddAttribute(new Attribute<float>("Health"));

            // create max health attribute
            Attribute<float> maxHealthAttribute = new Attribute<float>("MaxHealth");
            AddAttribute(maxHealthAttribute);

            // clamp health between zero and max health attribute value
            AddModifier(new ClampMaxModifier(new List<ModifierRequirement>() { new ModifierAttributeNameRequirement("Health"), new ModifierAttributeUserIdRequirement(uid) }, maxHealthAttribute)); // specific to this Module
            AddModifier(new ClampMinConstantModifier(new List<ModifierRequirement>() { new ModifierAttributeNameRequirement("Health"), new ModifierAttributeUserTypeRequirement(GetType()) }, 0f)); // applies to all Modules of same type
        }

        public virtual EntityHealth Initialise(Entity entity, float health, float maxHealth)
        {
            // set attribute values
            GetAttribute<Attribute<float>>("Health")?.SetBaseValue(health);
            GetAttribute<Attribute<float>>("MaxHealth")?.SetBaseValue(maxHealth);

            // register death event at 0 hp
            AddModifier(new TargetValueEventModifier<float>(new List<ModifierRequirement>() { new ModifierAttributeNameRequirement("Health"), new ModifierAttributeUserIdRequirement(uid) }, 0)).OnTriggered.AddListener((_) => entity.Die(), entity.uid + "-Death");

            // become contained by entity
            if (!entity.subModules.Contains(this))
                entity.subModules.Add(this);

            return this;
        }

        /// <summary>
        /// Increases health by specified amount.
        /// </summary>
        public virtual void AdjustHealth(float amount)
        {
            Attribute<float>? healthAttribute = GetAttribute<Attribute<float>>("Health");

            if (healthAttribute == null)
                return;

            // add health
            healthAttribute.SetBaseValue(healthAttribute.GetValue() + amount); // clamping is already handled by modifiers
        }
    }

    // EntityController (evaluates entity flags before passing on orders to EntityActions)

    public class EntityFlags : LoadedModule
    {
        public virtual EntityFlags Initialise(Entity entity)
        {
            // become contained by entity
            if (!entity.subModules.Contains(this))
                entity.subModules.Add(this);

            return this;
        }

        /// <returns>Value of Entity flag. Should be used instead of GetAttribute() to check flags.</returns>
        public bool GetFlag(string name, bool defaultValue)
        {
            // get or create flag (existing modifiers are immediately applied on creation)
            if (HasAttribute(name))
                return GetAttribute<Attribute<bool>>(name)?.GetValue() ?? defaultValue;
            else
            {
                Attribute<bool> flag = new Attribute<bool>(name, defaultValue);
                AddAttribute(flag);
                return flag.GetValue();
            }
        }
    }

    public class EntityActions : LoadedModule
    {
        protected Entity? entity;

        public virtual EntityActions Initialise(Entity entity)
        {
            this.entity = entity;

            // become contained by entity
            if (!entity.subModules.Contains(this))
                entity.subModules.Add(this);

            return this;
        }

        public virtual void StoreItem(Item item)
        {
            if (item == null) return;

            Inventory? container = entity?.subModules.FirstOrDefault(effect => effect is Inventory) as Inventory;
            if (container != null)
            {
                item.Stow();
                container.Store(item);
            }
        }

        public virtual void DropItem(Item item)
        {
            if (item == null) return;

            bool possessedItem = false;

            if (item is Equipment)
            {
                EntityEquipment? holder = entity?.subModules.FirstOrDefault(effect => effect is EntityEquipment) as EntityEquipment;
                if (holder != null)
                {
                    if (holder == ((Equipment)item).holder)
                        possessedItem = true;

                    holder.Unequip((Equipment)item);
                }
            }

            Inventory? container = entity?.subModules.FirstOrDefault(effect => effect is Inventory) as Inventory;
            if (container != null)
            {
                if (container == item.container)
                    possessedItem = true;

                container.Retrieve(item);
            }

            if (possessedItem)
                item.Materialise();
        }

        public virtual void EquipItem(Equipment equipment)
        {
            if (equipment == null) return;

            EntityEquipment? holder = entity?.subModules.FirstOrDefault(effect => effect is EntityEquipment) as EntityEquipment;
            if (holder != null && holder.GetEquipSlotAvailability(equipment.equipSlot) > 0)
            {
                equipment.Stow();
                holder.Equip(equipment);
            }
        }
    }

    public class EntityEquipment : LoadedModule
    {
        public Dictionary<string, int>? equipSlots { get; protected set; }

        public virtual EntityEquipment Initialise(Entity entity, Dictionary<string, int> equipSlots)
        {
            this.equipSlots = equipSlots;

            // become contained by entity
            if (!entity.subModules.Contains(this))
                entity.subModules.Add(this);

            return this;
        }

        public int GetEquipSlotAvailability(string equipSlot)
        {
            return (equipSlots?.ContainsKey(equipSlot) ?? false) ? equipSlots[equipSlot] - (subModules.Where(effect => effect is Equipment && ((Equipment)effect).equipSlot == equipSlot).Count()) : 0;
        }

        public virtual void Equip(Equipment equipment)
        {
            if (equipment == null) return;

            // remove equipment from previous inventory or equipment
            equipment.holder?.Unequip(equipment);
            equipment.container?.Retrieve(equipment);

            // add equipment
            subModules.Add(equipment);

            equipment.ChangeHolder(this);
        }

        public virtual void Unequip(Equipment equipment)
        {
            if (equipment == null) return;

            // remove equipment
            subModules.Remove(equipment);

            // unregister as holder of equipment
            if (equipment.holder == this)
                equipment.ChangeHolder(null);
        }

        public virtual Equipment[] GetInvalidEquipment() // (does this method have any purpose?)
        {
            return subModules.Where(effect => effect is Equipment && false).Select(effect => (Equipment)effect).ToArray() ?? new Equipment[0];
        }
    }

    public class Inventory : LoadedModule
    {
        public virtual void Store(Item item)
        {
            if (item == null) return;

            // remove item from previous inventory or equipment
            (item as Equipment)?.holder?.Unequip((Equipment)item);
            item.container?.Retrieve(item);

            // add item
            subModules.Add(item);

            item.ChangeContainer(this);
        }

        public virtual void Retrieve(Item item)
        {
            if (item == null) return;

            // remove item
            subModules.Remove(item);

            // unregister as container of item
            if (item.container == this)
                item.ChangeContainer(null);
        }
    }

    public abstract class Item : LoadedModule
    {
        public string displayName { get; protected set; } = "[item]";

        public Inventory? container { get; protected set; }

        public virtual void SetDisplayName(string name)
        {
            displayName = name;
        }

        public virtual void ChangeContainer(Inventory? container)
        {
            this.container = container;
        }

        public void Stow()
        {
            // hide object and disable physics
        }

        public void Materialise()
        {
            // show object and reenable physics
        }
    }

    public abstract class Equipment : Item
    {
        public abstract string equipSlot { get; protected set; }

        public EntityEquipment? holder { get; protected set; }

        public virtual void ChangeEquipSlot(string newSlot)
        {
            if (newSlot == equipSlot)
                return;

            equipSlot = newSlot;
        }

        public virtual void ChangeHolder(EntityEquipment? holder)
        {
            this.holder = holder;
        }
    }
}
