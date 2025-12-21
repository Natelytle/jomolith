using Godot;

namespace Jomolith.Editor.Models;

public partial class SelectionModel : RefCounted
{
    public int[] SelectedIds = [];
    public int PrimaryId = -1;
}