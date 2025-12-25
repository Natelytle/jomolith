using Godot;
using Jomolith.Editor.Commands;
using Jomolith.Editor.Models;

namespace Jomolith.Editor.Controllers;

public partial class MoveController : RefCounted
{
    private EditorContext _context = null!;

    public void Setup(EditorContext context)
    {
        _context = context;
    }

    public void OnDragStart(Vector3 hitPos)
    {
        
    }
    
    public void OnDragUpdate(Vector3 hitPos)
    {
        
    }
    
    public void OnDragEnd(Vector3 hitPos)
    {
        
    }
}