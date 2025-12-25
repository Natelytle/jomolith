using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Jomolith.Editor.Models;

public partial class SelectionModel : RefCounted
{
    #region Signals

    public event SelectionChangedEventHandler? SelectionChanged;
    public delegate void SelectionChangedEventHandler(int[] newSelection);
    
    public event SelectionsAddedEventHandler? SelectionsAdded;
    public delegate void SelectionsAddedEventHandler();

    public event SelectionsRemovedEventHandler? SelectionsRemoved;
    public delegate void SelectionsRemovedEventHandler(int[] ids);

    public event PrimaryIdChangedEventHandler? PrimaryIdChanged;
    public delegate void PrimaryIdChangedEventHandler(int? id);

    #endregion

    public readonly HashSet<int> SelectedIds = [];
    public int? PrimaryId;

    public void Add(int id)
    {
        SelectedIds.Add(id);
        SelectionChanged?.Invoke(SelectedIds.ToArray());
        
        SetPrimaryId(id);
    }

    public void AddMany(int[] ids, int? primaryId = null)
    {
        foreach (int id in ids) SelectedIds.Add(id);
        SelectionChanged?.Invoke(SelectedIds.ToArray());
        
        SetPrimaryId(primaryId);
    }

    public void Remove(int id)
    {
        SelectedIds.Remove(id);
        SelectionChanged?.Invoke(SelectedIds.ToArray());
        
        // Maybe I'll store a list of prev primary IDs and rollback to the previous one before this
        SetPrimaryId(null);
    }

    public void Clear()
    {
        SelectedIds.Clear();
        SelectionChanged?.Invoke(SelectedIds.ToArray());
        
        SetPrimaryId(null);
    }

    public void SetPrimaryId(int? id)
    {
        PrimaryId = id;

        PrimaryIdChanged?.Invoke(id);
    }
}