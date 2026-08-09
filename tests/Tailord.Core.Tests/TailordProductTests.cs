using Tailord.Core;
using Xunit;

namespace Tailord.Core.Tests;

public sealed class TailordProductTests
{
    [Fact]
    public void Name_IsStableForUserFacingClients()
    {
        Assert.Equal("Tailord", TailordProduct.Name);
    }
}

