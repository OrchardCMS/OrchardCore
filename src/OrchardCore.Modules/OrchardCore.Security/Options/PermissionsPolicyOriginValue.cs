namespace OrchardCore.Security
{
    public class PermissionsPolicyOriginValue
    {
        private readonly string _value;

        internal PermissionsPolicyOriginValue(string value) => _value = value;

        public static readonly PermissionsPolicyOriginValue Any = new("*");

        public static readonly PermissionsPolicyOriginValue Self = new("self");

        public static implicit operator string(PermissionsPolicyOriginValue option) => option.ToString();

        public override string ToString() => _value;
    }
}
