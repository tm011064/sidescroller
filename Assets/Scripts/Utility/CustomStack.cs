using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CustomStack<T> : List<T>
{
  public void Push(T obj)
  {
    this.Add(obj);
  }

  public T Pop()
  {
    if (this.Count > 0)
    {
      T item = this[this.Count - 1];
      this.Remove(item);
      return item;
    }

    return default(T);
  }
  
  public T Peek()
  {
    if (this.Count > 0)
    {
      return this[this.Count - 1];
    }

    return default(T);
  }


  public void Exchange(int index, T item)
  {
    if (index < this.Count)
    {
      this.Insert(index, item);
      this.RemoveAt(index + 1);
    }
  }

}

