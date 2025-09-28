namespace SGSClient.Controllers;

public class SGSVersion
{
    public readonly struct Version
    {
        public static readonly Version Zero = new(0, 0, 0);

        public short Major { get; }
        public short Minor { get; }
        public short SubMinor { get; }

        public Version(short major, short minor, short subMinor)
        {
            Major = major;
            Minor = minor;
            SubMinor = subMinor;
        }

        public Version(string version)
        {
            var parts = version.Split('.');
            if (parts.Length == 3
                && short.TryParse(parts[0], out var major)
                && short.TryParse(parts[1], out var minor)
                && short.TryParse(parts[2], out var subMinor))
            {
                Major = major;
                Minor = minor;
                SubMinor = subMinor;
            }
            else
            {
                Major = 0;
                Minor = 0;
                SubMinor = 0;
            }
        }

        public bool IsDifferentThan(Version other) =>
            !Equals(other);

        public override bool Equals(object obj) =>
            obj is Version v && Major == v.Major && Minor == v.Minor && SubMinor == v.SubMinor;

        public override int GetHashCode() =>
            HashCode.Combine(Major, Minor, SubMinor);

        public static bool operator ==(Version left, Version right) =>
            left.Equals(right);

        public static bool operator !=(Version left, Version right) =>
            !left.Equals(right);

        public override string ToString() =>
            $"{Major}.{Minor}.{SubMinor}";
    }
}
