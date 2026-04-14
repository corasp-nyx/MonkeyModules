# InteractiveComponents [Working Title]
Modular components with globally modifiable and dynamically reactive attributes.

Easily buildable into an Entity Component System useful for games with large amounts of dynamic ability stats and modifiers.

## Component Hierarchy
The core components of this library are Modules, Attributes, and Modifiers.

- Modules typically contain Attributes, create Modifiers, and execute the majority of your code.
- Attributes are typically used by Modules as dynamic variables, returning a value of set type which is updated whenever a change occurs in any targeting Modifier.
- Modifiers are typically applied to all Attributes matching their given requirements, and dynamically alter their values.

As alluded to with the term 'typically', everything is designed to be easily customisable into various other structures.

## Example Usage
For example, a game entity could have an EntityHealth Module, tracking its current and max health.

For doing so, the Module can call AddAttribute() to make those values available:

```C#
public class EntityHealth : LoadedModule
{
    public EntityHealth()
    {
        // create health attribute
        AddAttribute(new Attribute<float>("Health"));
        
        // create max health attribute
        Attribute<float> maxHealthAttribute = new Attribute<float>("MaxHealth"); // this will be used again in a moment
        AddAttribute(maxHealthAttribute);
```

Following this, we could add two Modifiers to automatically clamp the health Attribute between zero and the value of the max health Attribute at all times:

```C#
// clamp health between zero and max health attribute value

AddModifier(
    new ClampMaxModifier(
        new List<ModifierRequirement>() { // tell the Modifier which Attributes to target
            new ModifierAttributeNameRequirement("Health"),
            new ModifierAttributeUserIdRequirement(uid) }, // applies only to this Module (its immutable unique identifier)
        maxHealthAttribute)); // set maximum to value of max health Attribute

AddModifier(
    new ClampMinConstantModifier(
        new List<ModifierRequirement>() {
            new ModifierAttributeNameRequirement("Health"),
            new ModifierAttributeUserTypeRequirement(GetType()) }, // applies to all EntityHealth Modules (reduces duplicity)
        0f)); // set minimum to zero
```

Then we can initialise the Module with custom health and max health values, as well as passing along the parent entity, to which this EntityHealth Module belongs:

```C#
public void Initialise(EntityModule entity, float health, float maxHealth)
{
    // set attribute values
    GetAttribute<Attribute<float>>("Health")?.SetBaseValue(health);
    GetAttribute<Attribute<float>>("MaxHealth")?.SetBaseValue(maxHealth);

    // register death event at 0 health
    AddModifier(
        new TargetValueEventModifier<float>(
            new List<ModifierRequirement>() {
                new ModifierAttributeNameRequirement("Health"),
                new ModifierAttributeUserIdRequirement(uid) },
            0)).OnTriggered.AddListener((_) => entity.Die(), entity.uid + "-Death");

    // parent to entity
    if (!entity.subModules.Contains(this))
        entity.subModules.Add(this);
}
```

A barebones method for taking damage or healing can be as simple as this, as clamping, etc. already gets handled by modifiers:

```C#
public virtual void AdjustHealth(float amount)
{
    Attribute<float>? healthAttribute = GetAttribute<Attribute<float>>("Health");
    
    if (healthAttribute != null)
        healthAttribute.SetBaseValue(healthAttribute.GetValue() + amount);
}
```

An operation such as halving the entity's max health is now as easy as creating a single MultiplyModifier from anywhere:

```C#
AddModifier(
    new MultiplyModifier(
        new List<ModifierRequirement>() {
            new ModifierAttributeNameRequirement("MaxHealth"),
            new ModifierAttributeUserTypeRequirement(typeof(EntityHealth)),
            new ModifierAttributeUserParentIdRequirement(entity.uid) },
        0.5f));
```

Modifiers can be created and applied from any Module with this method, or added by calling GlobalManager.AddModifier() as an alternative. Various different ModifierRequirements can be used or made to accurately target the correct Attributes.

Multiple other examples are included in the Examples/ folder.

## (Work in progress)
