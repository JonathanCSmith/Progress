using System;
using System.Collections.Generic;

public class TopologicalSorter {

    private readonly int[] vertices;
    private readonly int[,] edgeMatrix;
    private readonly int[] sorted;

    private int currentCount = 0;

    public TopologicalSorter(int size) {
        this.vertices = new int[size];
        this.edgeMatrix = new int[size, size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                this.edgeMatrix[i, j] = 0;
            }
        }
        this.sorted = new int[size]; 
    }

    public int addVertex(int vertex) {
        this.vertices[this.currentCount++] = vertex;
        return vertex - 1;
    }

    public int addEdge(int start, int end) {
        this.edgeMatrix[start, end] = 1;
    }

    public int[] sort() {
        while (this.currentCount > 0) {
            // Find vertices with no remaining edges
            int currentVertex = this.findVertexWithoutEdge();
            if (currentVertex == -1) {
                throw new Exception("Cyclic dependency found");
            }

            // Insert label
            this.sorted[this.currentCount - 1] = this.vertices[currentVertex];
            this.deleteVertex(currentVertex);
        }

        return this.sorted;
    }

    public static List<string> sortByString(List<StringDependencyMap> maps) {
        TopologicalSorter sorter = new TopologicalSorter(maps.Count);

        Dictionary<string, int> indexes = new Dictionary<string, int>();

        // add vertices
        for (int i = 0; i < maps.Count; i++) {
            indexes[maps[i].name.ToLower()] = sorter.addVertex(i);
        }

        // add edges
        for (int i = 0; i < maps.Count; i++) {
            if (maps[i].dependencies != null) {
                for (int j = 0; j < maps[i].dependencies.Length; j++) {
                    sorter.addEdge(i, indexes[maps[i].dependencies[j].ToLower()]);
                }
            }
        }

        // Sort and return null if it fails
        try {
            int[] results = sorter.sort();
        }

        catch {
            return null;
        }

        // output
        List<string> output = new List<string>();
        foreach (int resultIndex in results) {
            output.Add(maps[resultIndex].name);
        }
        return output;
    }

    private int findVertexWithoutEdge() {
        for (int row = 0; row < this.currentCount; row++) {
            bool hasEdge = false;
            for (int col = 0; col < this.currentCount; col++) {
                if (this.edgeMatrix[row, col] > 0) {
                    hasEdge = true;
                    break;
                }
            }

            if (!hasEdge) {
                return row;
            }
        }

        return -1; // NOOP
    }

    private void deleteVertex(int index) {
        if (index != this.currentCount - 1) { 
            for (int j = index; j < this.currentCount - 1; j++) {
                this.vertices[j] = this.vertices[j + 1];
            }

            for (int row = index; row < this.currentCount - 1; row++) {
                this.moveRowUp(row, this.currentCount);
            }

            for (int col = index; col < this.currentCount - 1; col++) {
                this.moveColumnLeft(col, this.currentCount - 1);
            }
        }

        this.currentCount--;
    }

    private void moveRowUp(int row, int length) {
        for (int col = 0; col < length; col++) {
            this.edgeMatrix[row, col] = this.edgeMatrix[row + 1, col];
        }
    }

    private void moveColumnLeft(int col, int length) {
        for (int row = 0; row < length; row++) {
            this.edgeMatrix[row, col] = this.edgeMatrix[row, col + 1];
        }
    }
}

public class StringDependencyMap {
    public string name { get; set; }
    public string[] dependencies { get; set; }
}

