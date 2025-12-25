using System;
using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Controllers;

public partial class SelectionController : RefCounted
{
    private EditorContext _context = null!;

    public void Setup(EditorContext context)
    {
        _context = context;
    }

    public void OnClick(int id, bool additive)
    {
        GD.Print("On Click activated");
    }
}