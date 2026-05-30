# MonkeyModules [Working Title]
Systemic modules with globally modifiable and dynamically reactive attributes.

Easily buildable into an Entity Component System useful for games with large amounts of dynamic ability stats and modifiers.

## Component Hierarchy
The core components of this library are Modules, Attributes, and Modifiers.

- Modules typically contain Attributes, create Modifiers, and execute the majority of your code.
- Attributes are typically used by Modules as dynamic variables, returning a value of set type which is updated whenever a change occurs in any targeting Modifier.
- Modifiers are typically applied to all Attributes matching their given requirements, and dynamically alter their values.

As alluded to with the term 'typically', everything is designed to be easily customisable into various other structures.

## Example Usage
For example, a game entity could have an EntityHealth Module, tracking its current and max health.

For doing so, the Module can call AddAttribute() to make those values available and immediately assign them:

```C#
public class EntityHealth : LoadedModule
{
    public EntityHealth(Entity entity, float health = 1f, float maxHealth = 1f)
    {
        // create health attribute
        AddAttribute(new Attribute<float>("Health", health));
        
        // create max health attribute
        Attribute<float> maxHealthAttribute = new Attribute<float>("MaxHealth", maxHealth);
        AddAttribute(maxHealthAttribute);
```

Following this, we could add two Modifiers to automatically clamp the health Attribute between zero and the value of the max health Attribute at all times:

```C#
    // clamp health between zero and max health attribute value
    
    AddModifier(
        new ClampMaxModifier(
            new ModifierRequirement[] { // tells the Modifier which Attributes to target
                new ModifierAttributeNameRequirement("Health"), // Attribute names are immutable and unique per Module
                new ModifierAttributeUserIdRequirement(uid) }, // each Module, Attribute, and Modifier has a globally unique immutable id
            maxHealthAttribute)); // set maximum to value of max health Attribute
    
    AddModifier(
        new ClampMinConstantModifier(
            new ModifierRequirement[] {
                new ModifierAttributeNameRequirement("Health"),
                new ModifierAttributeUserTypeRequirement(GetType()) }, // applies to all EntityHealth Modules (reduces duplicity)
            0f)); // set minimum to zero
```

Then we can add a useful event which automatically triggers an entity's death at zero health, as well as parent this Module to that entity:

```C#
    // register death event at 0 health
    AddModifier(
        new TargetValueEventModifier<float>(
            new ModifierRequirement[] {
                new ModifierAttributeNameRequirement("Health"),
                new ModifierAttributeUserIdRequirement(uid) },
            0)).OnTriggered.AddListener((_) => entity.Die(), entity.uid + "-Death");

    // parent to entity
    if (!entity.subModules.Contains(this))
        {
            entity.subModules.Add(this);
            entity.OnDecommission.AddListener(Decommission, uid + decommissionEventKeySuffix);
        }
}
```

A barebones method for taking damage or healing can be as simple as the following, as clamping, etc. already gets handled by the Modifiers:

```C#
public virtual void AdjustHealth(float amount)
{
    Attribute<float>? healthAttribute = GetAttribute<Attribute<float>>("Health");
    
    if (healthAttribute != null)
        healthAttribute.SetBaseValue(healthAttribute.GetBaseValue() + amount);
}
```

An operation such as halving the entity's max health can be done by creating a single MultiplyModifier from anywhere:

```C#
AddModifier(
    new MultiplyModifier(
        new ModifierRequirement[] {
            new ModifierAttributeNameRequirement("MaxHealth"),
            new ModifierAttributeUserTypeRequirement(typeof(EntityHealth)),
            new ModifierAttributeUserIdRequirement(uid) },
        0.5f));
```

Modifiers can be created and applied from any Module with this method, or added by calling GlobalManager.AddModifier() as an alternative. Various different ModifierRequirements can be used or made to accurately target the correct Attributes.

Further examples can be found in the /Examples folder.

## (Work in progress)
