using System;

namespace CursosIglesia.Models;

public static class Roles
{
    public static readonly Guid Estudiante = Guid.Parse("DE600C8E-567A-4ABB-8C47-1EF4A6807878");
    public static readonly Guid SuperAdmin = Guid.Parse("5B664D1E-C1F4-4EA9-934E-262145ACE2D0");
    public static readonly Guid Maestro = Guid.Parse("C28185B6-9A38-4AE7-BCAF-DCFE1E1848D1");

    public static bool IsStaff(Guid roleId)
    {
        return roleId == SuperAdmin || roleId == Maestro;
    }
}
