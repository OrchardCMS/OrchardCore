using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.DisplayManagement
{
    public class ZoneOnDemandTests
    {
        [Fact]
        public void ZoneOnDemandIsEqualToNull()
        {
            var zoneOnDemand = CreateZoneOnDemand("SomeZone");

            Assert.True(zoneOnDemand == null);
            Assert.False(zoneOnDemand != null);
            Assert.True(zoneOnDemand == Nil.Instance);
            Assert.False(zoneOnDemand != Nil.Instance);
        }

        [Fact]
        public void DynamicZoneOnDemandIsEqualToNull()
        {
            dynamic zoneOnDemand = CreateZoneOnDemand("SomeZone");

            Assert.True(zoneOnDemand == null);
            Assert.False(zoneOnDemand != null);
            Assert.True(zoneOnDemand == Nil.Instance);
            Assert.False(zoneOnDemand != Nil.Instance);
        }

        [Fact]
        public void ZoneOnDemandAsBaseTypeIsNotEqualToNull()
        {
            var zoneOnDemand = CreateZoneOnDemand("SomeZone");

            Shape zoneShape = zoneOnDemand;
            Composite zoneComposite = zoneOnDemand;
            object zoneObject = zoneOnDemand;

            Assert.False(zoneShape == null);
            Assert.False(zoneComposite == null);
            Assert.False(zoneObject == null);
        }

        [Fact]
        public void ZoneOnDemandIsNotEqualToItself()
        {
            var zoneOnDemand1 = CreateZoneOnDemand("SomeZone");
            var zoneOnDemand2 = zoneOnDemand1;

            Assert.False(zoneOnDemand1 == zoneOnDemand2);
            Assert.True(ReferenceEquals(zoneOnDemand1, zoneOnDemand2));
        }

        [Fact]
        public void DynamicZoneOnDemandIsNotEqualToItself()
        {
            dynamic zoneOnDemand1 = CreateZoneOnDemand("SomeZone");
            dynamic zoneOnDemand2 = zoneOnDemand1;

            Assert.False(zoneOnDemand1 == zoneOnDemand2);
            Assert.True(ReferenceEquals(zoneOnDemand1, zoneOnDemand2));
        }

        [Fact]
        public void ZoneOnDemandAsBaseTypeIsEqualToItself()
        {
            var zoneOnDemand = CreateZoneOnDemand("SomeZone");

            Shape zoneShape1 = zoneOnDemand, zoneShape2 = zoneOnDemand;
            Composite zoneComposite1 = zoneOnDemand, zoneComposite2 = zoneOnDemand;
            object zoneObject1 = zoneOnDemand, zoneObject2 = zoneOnDemand;

            // Intended reference comparison.
            Assert.True(zoneShape1 == zoneShape2);
            Assert.True(zoneComposite1 == zoneComposite2);
            Assert.True(zoneObject1 == zoneObject2);
        }

        [Fact]
        public void ZoneHoldingPropertiesMissingIndexThrows()
        {
            Assert.Throws<KeyNotFoundException>(() => { _ = CreateZoneHolding().Properties["SomeZone"]; });
        }

        [Fact]
        public void DynamicZoneHoldingMissingIndexIsNull()
        {
            dynamic zoneHolding = CreateZoneHolding();
            var zone = zoneHolding["SomeZone"];

            Assert.False(zone is ZoneOnDemand);
            Assert.True(zone == null);
        }

        [Fact]
        public void DynamicZoneHoldingMissingPropertyAddsAnEmptyZoneOnDemand()
        {
            dynamic zoneHolding = CreateZoneHolding();
            var someZone = zoneHolding.SomeZone;

            Assert.True(someZone is ZoneOnDemand);
            Assert.True(someZone == null);

            Assert.True(Object.ReferenceEquals(zoneHolding.SomeZone, someZone));
            Assert.True(Object.ReferenceEquals(zoneHolding["SomeZone"], someZone));
            Assert.True(Object.ReferenceEquals((zoneHolding as ZoneHolding).Properties["SomeZone"], someZone));

            var dynamicZone = zoneHolding["SomeZone"];
            Assert.True(dynamicZone is ZoneOnDemand);
            Assert.True(dynamicZone == null);

            var zoneObject = (zoneHolding as ZoneHolding).Properties["SomeZone"];
            Assert.True(zoneObject is ZoneOnDemand);
            Assert.False(zoneObject == null);
        }

        [Fact]
        public void DynamicZonesMissingPropertyReturnsATemporaryZoneOnDemand()
        {
            dynamic zoneHolding = CreateZoneHolding();
            var zoneOnDemand = zoneHolding.Zones.SomeZone;

            Assert.True(zoneOnDemand is ZoneOnDemand);
            Assert.True(zoneOnDemand == null);

            Assert.False(Object.ReferenceEquals(zoneHolding.Zones.SomeZone, zoneOnDemand));

            var someZone = zoneHolding["SomeZone"];

            Assert.False(someZone is ZoneOnDemand);
            Assert.False(someZone is Shape);
            Assert.True(someZone == null);
        }

        [Fact]
        public async Task AddingAnItemToAZoneOnDemandAddsAZoneShapeToItsParent()
        {
            dynamic zoneHolding = CreateZoneHolding();
            var zoneOnDemand = zoneHolding.Zones.SomeZone;

            await zoneOnDemand.AddAsync(new object());

            var someZone = zoneHolding["SomeZone"];

            Assert.False(someZone is ZoneOnDemand);
            Assert.True(someZone is Shape);
            Assert.False(someZone == null);

            Assert.True(Object.ReferenceEquals(zoneHolding.SomeZone, someZone));
            Assert.True(Object.ReferenceEquals(zoneHolding["SomeZone"], someZone));
            Assert.True(Object.ReferenceEquals(zoneHolding.Zones.SomeZone, someZone));
            Assert.True(Object.ReferenceEquals(zoneHolding.Zones["SomeZone"], someZone));
            Assert.True(Object.ReferenceEquals((zoneHolding as ZoneHolding).Properties["SomeZone"], someZone));
        }

        [Fact]
        public void DynamicZoneOnDemandShouldBeRecursive()
        {
            dynamic zoneOnDemand = CreateZoneOnDemand("SomeZone");

            Assert.True(zoneOnDemand.Foo == null);
            Assert.True(zoneOnDemand.Foo.Bar == null);
            Assert.True(zoneOnDemand.Foo.Bar == Nil.Instance);
        }

        [Fact]
        public void CallingToStringOnZoneOnDemandShouldReturnEmpty()
        {
            var zoneOnDemand = CreateZoneOnDemand("SomeZone");
            Assert.Equal("", zoneOnDemand.ToString());
        }

        [Fact]
        public void CallingToStringOnDynamicZoneOnDemandShouldReturnEmpty()
        {
            dynamic zoneOnDemand = CreateZoneOnDemand("SomeZone");
            Assert.Equal("", zoneOnDemand.Foo.Bar.ToString());
        }

        [Fact]
        public void ConvertingZoneOnDemandToStringShouldReturnNullString()
        {
            dynamic zoneOnDemand = CreateZoneOnDemand("SomeZone");
            Assert.True((string)zoneOnDemand == null);
        }

        private static ZoneHolding CreateZoneHolding() => new(() => new ValueTask<IShape>(new Shape()));

        private static ZoneOnDemand CreateZoneOnDemand(string name, ZoneHolding zoneHolding = null) =>
            new(() => new ValueTask<IShape>(new Shape()), zoneHolding ?? CreateZoneHolding(), name);
    }
}
