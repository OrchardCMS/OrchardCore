using System;

namespace OrchardCore.Environment.Shell.Descriptor.Models
{
    public class ShellFeature : IEquatable<ShellFeature>
    {
        public ShellFeature()
        {
        }

        public ShellFeature(string id, bool alwaysEnabled = false)
        {
            Id = id;
            AlwaysEnabled = alwaysEnabled;
        }

        public string Id { get; set; }
        public bool AlwaysEnabled { get; set; }

        public bool Equals(ShellFeature other)
        {
            if (other == null)
            {
                return false;
            }

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
