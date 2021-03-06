﻿using System;
#if (NET46)
using ApprovalTests;
#endif
using Xunit;

public class RewritingProperties
{
    [Fact]
    public void PropertySetterRequiresNonNullArgument()
    {
        var type = AssemblyWeaver.Assembly.GetType("SimpleClass");
        var sample = (dynamic)Activator.CreateInstance(type);
        var exception = Assert.Throws<ArgumentNullException>(() => { sample.NonNullProperty = null; });
#if (NET46)
        Approvals.Verify(exception.Message);
#endif
    }

    [Fact]
    public void PropertyGetterRequiresNonNullReturnValue()
    {
        var type = AssemblyWeaver.Assembly.GetType("SimpleClass");
        var sample = (dynamic)Activator.CreateInstance(type);
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            // ReSharper disable UnusedVariable
            var temp = sample.NonNullProperty;

            // ReSharper restore UnusedVariable
        });
#if (NET46)
        Approvals.Verify(exception.Message);
#endif
    }

    [Fact]
    public void GenericPropertyGetterRequiresNonNullReturnValue()
    {
        var type = AssemblyWeaver.Assembly.GetType("GenericClass`1");
        var sample = (dynamic)Activator.CreateInstance(type.MakeGenericType(typeof(string)));
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            // ReSharper disable UnusedVariable
            var temp = sample.NonNullProperty;

            // ReSharper restore UnusedVariable
        });
#if (NET46)
        Approvals.Verify(exception.Message);
#endif
    }

    [Fact]
    public void PropertyAllowsNullGetButNotSet()
    {
        var type = AssemblyWeaver.Assembly.GetType("SimpleClass");
        var sample = (dynamic)Activator.CreateInstance(type);
        Assert.Null(sample.PropertyAllowsNullGetButDoesNotAllowNullSet);
        var exception = Assert.Throws<ArgumentNullException>(() => { sample.NonNullProperty = null; });
#if (NET46)
        Approvals.Verify(exception.Message);
#endif
    }

    [Fact]
    public void PropertyAllowsNullSetButNotGet()
    {
        var type = AssemblyWeaver.Assembly.GetType("SimpleClass");
        var sample = (dynamic)Activator.CreateInstance(type);
        sample.PropertyAllowsNullSetButDoesNotAllowNullGet = null;
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            // ReSharper disable UnusedVariable
            var temp = sample.PropertyAllowsNullSetButDoesNotAllowNullGet;

            // ReSharper restore UnusedVariable
        });
#if (NET46)
        Approvals.Verify(exception.Message);
#endif
    }

    [Fact]
    public void PropertySetterRequiresAllowsNullArgumentForNullableType()
    {
        var type = AssemblyWeaver.Assembly.GetType("SimpleClass");
        var sample = (dynamic)Activator.CreateInstance(type);
        sample.NonNullNullableProperty = null;
    }

    [Fact]
    public void DoesNotRequireNullSetterWhenPropertiesNotSpecifiedByAttribute()
    {
        var type = AssemblyWeaver.Assembly.GetType("ClassWithPrivateMethod");
        var sample = (dynamic)Activator.CreateInstance(type);
        sample.SomeProperty = null;
    }

    [Fact]
    public void AllowsNullWhenClassMatchExcludeRegex()
    {
        var type = AssemblyWeaver.Assembly.GetType("ClassToExclude");
        var classToExclude = (dynamic) Activator.CreateInstance(type, "");
        classToExclude.Property = null;
        string result = classToExclude.Property;
    }
}