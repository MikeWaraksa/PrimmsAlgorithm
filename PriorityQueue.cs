using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<T> {

    struct PElement {
        public float priority;
        public T data;

        public PElement (float f, T d) {
            priority = f;
            data = d;
        }
    }

    List<PElement> PList = new List<PElement>();

    public void Add (T d, float priority) {
        int i;
        for (i = 0; i < PList.Count; i++) {
            if (priority < PList[i].priority) break;
        }

        PList.Insert(i, new PElement(priority, d));
    }

    public void RemoveAt (int index) {
        PList.RemoveAt(index);
    }

    public T Get (int index) {
        return PList[index].data;
    }

    public float GetPriority(int index) {
        return PList[index].priority;
    }

    public int Count {
        get { return PList.Count; }
    }
}