[![Chat on Gitter](https://img.shields.io/gitter/room/fody/fody.svg?style=flat)](https://gitter.im/Fody)
[![NuGet Status](http://img.shields.io/nuget/v/NullGuard.Fody.svg?style=flat)](https://www.nuget.org/packages/NullGuard.Fody/)


## This is an add-in for [Fody](https://github.com/Fody/Fody/)

[Introduction to Fody](http://github.com/Fody/Fody/wiki/SampleUsage)


## The nuget package

https://nuget.org/packages/NullGuard.Fody/

```powershell
PM> Install-Package NullGuard.Fody
```


## Modes

As of v1.6.3 NullGuard supports two modes of operations, [*implicit*](#implicit-mode) and [*explicit*](#explicit-mode).

 * In [*implicit*](#implicit-mode) mode everything is assumed to be not-null, unless attributed with `[AllowNull]`. This is how NullGuard has been working always.
 * In the new [*explicit*](#explicit-mode) mode everything is assumed to be nullable, unless attributed with `[NotNull]`. This mode is designed to support the R# nullability analysis, using pessimistic mode.

If not configured explicitly, NullGuard will auto-detect the mode as follows:

 * Referencing `JetBrains.Annotations` and using `[NotNull]` anywhere will switch to explicit mode.
 * Default to implicit mode if the above criteria is not met.


### Implicit Mode


#### Your Code


```csharp
public class Sample
{
    public void SomeMethod(string arg)
    {
        // throws ArgumentNullException if arg is null.
    }

    public void AnotherMethod([AllowNull] string arg)
    {
        // arg may be null here
    }

    public string MethodWithReturn()
    {
        return SomeOtherClass.SomeMethod();
    }

    [return: AllowNull]
    public string MethodAllowsNullReturnValue()
    {
        return null;
    }

    // Null checking works for automatic properties too.
    public string SomeProperty { get; set; }

    // can be applied to a whole property
    [AllowNull] 
    public string NullProperty { get; set; }

    // Or just the setter.
    public string NullPropertyOnSet { get; [param: AllowNull] set; }
}
```


#### What gets compiled 

```csharp
public class SampleOutput
{

    public string NullProperty{get;set}

    string nullPropertyOnSet;
    public string NullPropertyOnSet
    {
        get
        {
            var returnValue = nullPropertyOnSet;
            if (returnValue == null)
            {
                throw new InvalidOperationException("Return value of property 'NullPropertyOnSet' is null.");
            }
            return returnValue;
        }
        set
        {
            nullPropertyOnSet = value;
        }
    }

    public string MethodAllowsNullReturnValue()
    {
        return null;
    }

    string someProperty;
    public string SomeProperty
    {
        get
        {
            if (someProperty == null)
            {
                throw new InvalidOperationException("Return value of property 'SomeProperty' is null.");
            }
            return someProperty;
        }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value", "Cannot set the value of property 'SomeProperty' to null.");
            }
            someProperty = value;
        }
    }

    public void AnotherMethod(string arg)
    {
    }

    public string MethodWithReturn()
    {
        var returnValue = SomeOtherClass.SomeMethod();
        if (returnValue == null)
        {
            throw new InvalidOperationException("Return value of method 'MethodWithReturn' is null.");
        }
        return returnValue;
    }

    public void SomeMethod(string arg)
    {
        if (arg == null)
        {
            throw new ArgumentNullException("arg");
        }
    }
}
```


### Explicit Mode

If you are (already) using R#'s `[NotNull]` attribute in your code to explicitly annotate not null items, 
null guards will be added only for items that have an explicit `[NotNull]` annotation.


```csharp
public class Sample
{
    public void SomeMethod([NotNull] string arg)
    {
        // throws ArgumentNullException if arg is null.
    }

    public void AnotherMethod(string arg)
    {
        // arg may be null here
    }

    [NotNull]
    public string MethodWithReturn()
    {
        return SomeOtherClass.SomeMethod();
    }

    public string MethodAllowsNullReturnValue()
    {
        return null;
    }

    // Null checking works for automatic properties too.
    // Default in explicit mode is nullable
    public string NullProperty { get; set; }

    // NotNull can be applied to a whole property
    [NotNull]
    public string SomeProperty { get; set; }

    // or just the getter by overwriting the set method,
    [NotNull]
    public string NullPropertyOnSet { get; [param: AllowNull] set; }

    // or just the setter by overwriting the get method.
    [NotNull]
    public string NullPropertyOnGet { [return: AllowNull] get; set; }
}
```


Inheritance of nullability is supported in explicit mode, i.e. if you implement an interface or derive from a base method with `[NotNull]` annotations, 
null guards will be added to your implementation.

You may use the `[NotNull]` attribute defined in `JetBrains.Anntotations`, or simply define your own. However not referencing `JetBrains.Anntotations` will not auto-detect explicit mode, so you have to set this in the configuration.

Also note that using `JetBrains.Anntotations` will require to define [`JETBRAINS_ANNOTATIONS`](https://www.jetbrains.com/help/resharper/Code_Analysis__Annotations_in_Source_Code.html) to include the attributes in the assembly, so NullGuard can find them.
NullGuard will neither remove those attributes nor the reference to  `JetBrains.Anntotations`. To get rid of the attributes and the reference, you can use [JetBrainsAnnotations.Fody](https://github.com/tom-englert/JetBrainsAnnotations.Fody).
Just make sure NullGuard will run prior to [JetBrainsAnnotations.Fody](https://github.com/tom-englert/JetBrainsAnnotations.Fody).


## Attributes

Where and how injection occurs can be controlled via attributes. The NullGuard.Fody nuget ships with an assembly containing these attributes.

```csharp
namespace NullGuard
{
    /// <summary>
    /// Prevents the injection of null checking.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.Property)]
    public class AllowNullAttribute : Attribute
    {
    }
    
    /// <summary>
    /// Allow specific categories of members to be targeted for injection. <seealso cref="ValidationFlags"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
    public class NullGuardAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullGuardAttribute"/> with a <see cref="ValidationFlags"/>.
        /// </summary>
        /// <param name="flags">The <see cref="ValidationFlags"/> to use for the target this attribute is being applied to.</param>
        public NullGuardAttribute(ValidationFlags flags)
        {
        }
    }
    
    /// <summary>
    /// Used by <see cref="NullGuardAttribute"/> to target specific categories of members.
    /// </summary>
    [Flags]
    public enum ValidationFlags
    {
        None = 0,
        Properties = 1,
        Arguments = 2,
        OutValues = 4,
        ReturnValues = 8,
        NonPublic = 16,
        Methods = Arguments | OutValues | ReturnValues,
        AllPublicArguments = Properties | Arguments,
        AllPublic = Properties | Methods,
        All = AllPublic | NonPublic
    }
}
```


## Configuration

For Release builds NullGuard will weave code that throws `ArgumentNullException`. For Debug builds NullGuard weaves `Debug.Assert`. 
If you want ArgumentNullException to be thrown for Debug builds then update FodyWeavers.xml to include:

```xml
<NullGuard IncludeDebugAssert="false" />
```

A complete example of `FodyWeavers.xml` looks like this:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Weavers>
    <NullGuard IncludeDebugAssert="false" />
</Weavers>
```

You can also use RegEx to specify the name of a class to exclude from NullGuard.

```xml
<NullGuard ExcludeRegex="^ClassToExclude$" />
```

You can force the operation mode by setting it to `Explicit` or `Implicit`, if the default `AutoDetect` does not detect the usage correctly.

```xml
<NullGuard Mode="Explicit" />
```


## Contributors

  * [Cameron MacFarland](https://github.com/distantcam)
  * [Simon Cropp](https://github.com/simoncropp)
  * [Tim Murphy](https://github.com/TimMurphy)
  * [Tom Englert](https://github.com/tom-englert)


## Icon

Icon courtesy of [The Noun Project](http://thenounproject.com)
