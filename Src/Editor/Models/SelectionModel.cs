using System.Collections.Generic;
using Godot;

namespace Jomolith.Editor.Models;

public partial class SelectionModel : RefCounted
{
    public readonly List<int> SelectedIds = [];
    public int? PrimaryId;

    public void Clear()
    {
        SelectedIds.Clear();
        PrimaryId = null;
    }
}