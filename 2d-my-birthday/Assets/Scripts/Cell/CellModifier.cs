using System;

[Flags]
public enum CellModifier
{
    None = 0,
    Bonus = 1 << 0,
    Blocked = 1 << 1,
    Special = 1 << 2,
    Weekend = 1 << 3, // yeni: haftasonu (kýrmýzý)
    Skipped = 1 << 4
}