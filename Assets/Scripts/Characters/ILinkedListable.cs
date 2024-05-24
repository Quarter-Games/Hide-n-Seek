using System;
using Unity.VisualScripting;
using UnityEngine;

public interface ILinkedListable
{
    public Action<int> CountChanged { get; set; }
    public ILinkedListable Next { get; set; }
    public ILinkedListable Previous { get; set; }
    virtual void Remove()
    {
        if (Previous != null)
        {
            Previous.Next = Next;
        }
        if (Next != null)
        {
            Next.Previous = Previous;
        }
    }
    public virtual ILinkedListable GetLast()
    {
        ILinkedListable last = this;
        while (last.Next != null)
        {
            last = last.Next;
        }
        return last;
    }
    public virtual ILinkedListable GetFirst()
    {
        ILinkedListable first = this;
        while (first.Previous != null)
        {
            first = first.Previous;
        }
        return first;
    }
    public virtual void AddAfter(ILinkedListable node)
    {
        this.Next = node.Next;
        if (Next != null) node.Next.Previous = this;
        node.Next = this;
        this.Previous = node;
        var first = GetFirst();
        first.CountChanged?.Invoke(first.Count());
    }
    public int Count()
    {
        int count = 1;
        ILinkedListable current = this;
        while (current.Next != null)
        {
            count++;
            current = current.Next;
        }
        return count;
    }
    public virtual void RemoveAllAfter()
    {
        var temp = Previous;
        if (Previous != null) Previous.Next = null;
        Previous = null;
        if (Next != null)
        {
            Next.RemoveAllAfter();
        }
        if (temp!=null) temp.GetFirst().CountChanged?.Invoke(Count());
    }

}
