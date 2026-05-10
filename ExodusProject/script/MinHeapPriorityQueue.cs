using System.Collections.Generic;

public class MinHeapPriorityQueue
{
    private List<NodePriority> heap = new List<NodePriority>();
    public int Count => heap.Count;

    public void Enqueue(int id, float priority)
    {
        heap.Add(new NodePriority(id, priority));
        int current = heap.Count - 1;

        while (current > 0)
        {
            int parent = (current - 1) / 2;
            if (heap[current].priority >= heap[parent].priority)
                break;

            var temp = heap[current];
            heap[current] = heap[parent];
            heap[parent] = temp;

            current = parent;
        }
    }

    public int Dequeue()
    {
        int rootId = heap[0].id;
        heap[0] = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        int current = 0;
        while (true)
        {
            int left = 2 * current + 1;
            int right = 2 * current + 2;
            int smallest = current;

            if (left < heap.Count && heap[left].priority < heap[smallest].priority)
                smallest = left;

            if (right < heap.Count && heap[right].priority < heap[smallest].priority)
                smallest = right;

            if (smallest == current)
                break;

            var temp = heap[current];
            heap[current] = heap[smallest];
            heap[smallest] = temp;

            current = smallest;
        }

        return rootId;
    }

    private struct NodePriority
    {
        public int id; 
        public float priority;
        public NodePriority(int i, float p) { id = i; priority = p; }
    }
}