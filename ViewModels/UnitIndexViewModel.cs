using System;
using System.Collections.Generic;
using Rental.Models;

namespace Rental.ViewModels;

public class UnitIndexViewModel
{
    public IReadOnlyList<Unit> Units { get; init; } = Array.Empty<Unit>();
    public IReadOnlyList<Property> Properties { get; init; } = Array.Empty<Property>();
    public int? SelectedPropertyId { get; init; }
}
