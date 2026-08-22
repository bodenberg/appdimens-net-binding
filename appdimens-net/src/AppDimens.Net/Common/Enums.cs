namespace AppDimens.Net.Common;

/// <summary>Screen axis used as scaling reference. Mirrors Kotlin <c>DpQualifier</c>.</summary>
public enum DpQualifier
{
    SmallWidth = 0,
    Height = 1,
    Width = 2,
}

/// <summary>Orientation-swap rule applied before reading the screen dimension.</summary>
public enum Inverter
{
    Default = 0,
    PhToLw = 1,
    PwToLh = 2,
    LhToPw = 3,
    LwToPh = 4,
    SwToLh = 5,
    SwToLw = 6,
    SwToPh = 7,
    SwToPw = 8,
}

/// <summary>Target orientation for facilitators. Mirrors Kotlin <c>Orientation</c>.</summary>
public enum Orientation
{
    Portrait = 0,
    Landscape = 1,
    Default = 2,
}

/// <summary>UI mode type, including foldable/flip synthetic states (KMP parity).</summary>
public enum UiModeType
{
    Undefined = 0,
    Normal = 1,
    Television = 2,
    Car = 3,
    Watch = 4,
    Desk = 5,
    Appliance = 6,
    VrHeadset = 7,
    FoldOpen = -101,
    FoldClosed = -102,
    FlipOpen = -103,
    FlipClosed = -104,
    FoldHalfOpened = -105,
    FlipHalfOpened = -106,
}

/// <summary>Physical unit discriminator for the units module.</summary>
public enum UnitType
{
    Mm = 0,
    Cm = 1,
    Inch = 2,
    Dp = 3,
    Sp = 4,
    Px = 5,
}
