﻿using System.Diagnostics.CodeAnalysis;
using System.Text;
using OwaspHeaders.Core.Extensions;
using Xunit;

namespace tests;

[ExcludeFromCodeCoverage]
public class StringBuilderExtensionTests
{
    [Fact]
    public void TrimEnd_StringBuilder_Is_Null_Return_StringBuilder()
    {
        // arrange & act
        var builder = ((StringBuilder)null).TrimEnd();

        // assert
        Assert.Null(builder);
    }
    
    [Fact]
    public void TrimEnd_StringBuilder_Is_Empty_Return_StringBuilder()
    {
        // arrange
        StringBuilder builder = new StringBuilder();

        // act
        builder = builder.TrimEnd();

        // assert
        Assert.NotNull(builder);
        Assert.True(builder.Length == 0);
    }
}
