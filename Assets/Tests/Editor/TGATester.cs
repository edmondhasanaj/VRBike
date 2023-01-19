using UnityEngine;
using Engine.City;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class TGATester {
    ChunkData GenerateChunkData(params ChunkConnector[] allC)
    {
        return new ChunkData(new GameObject(), Vector2Int.one * 10, Vector2Int.one * 10, allC);
    }
    
    /// <summary>
    /// Tests if 2 Chunks with 1 Connector each at specific points can be snapped
    /// at specific directions.
    /// Chunk1 -> Connector is to the right at (9,8)
    /// Chunk2 -> Connector is to the left at (0,9)
    /// If we were to snap Chunk1 to Chunk2 in the right direction, the algorithm 
    /// should return true(this operation is doable) and also the rotation of Chunk2 
    /// should be 0.
    /// </summary>
    [Test]
    public void Test1() {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 8), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.left, new Vector2Int(0, 8), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 1, "The rotations length was " + rot.Length);
        Assert.AreEqual(0f, rot[0], 0.1f);
    }

    [Test]
    public void Test2()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 8), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.left, new Vector2Int(0, 2), 1));


        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.IsNull(rot);
    }

    [Test]
    public void Test3()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 8), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 8), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.IsNull(rot);
    }

    [Test]
    public void Test4()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 8), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 1), 1));


        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 1, "The rotations length was " + rot.Length);
        Assert.AreEqual(180f, rot[0], 0.1f);
    }

    [Test]
    public void Test5()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 5), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.up, new Vector2Int(5, 9), 1));


        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 1, "The rotations length was " + rot.Length);
        Assert.AreEqual(90f, rot[0], 0.1f);
    }

    [Test]
    public void Test6()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 5), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.up, new Vector2Int(4, 9), 1));


        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.IsNull(rot);
    }

    [Test]
    public void Test7()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 5), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.up, new Vector2Int(5, 9), 1), new ChunkConnector(Vector2Int.left, new Vector2Int(0,5), 1), new ChunkConnector(Vector2Int.down, new Vector2Int(4, 0), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 3, "The rotations length was " + rot.Length);
        Assert.Contains(0f, rot);
        Assert.Contains(90f, rot);
        Assert.Contains(270f, rot);
    }

    [Test]
    public void Test8()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 5), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.up, new Vector2Int(5, 9), 1), new ChunkConnector(Vector2Int.up, new Vector2Int(6, 9), 1), new ChunkConnector(Vector2Int.left, new Vector2Int(0, 5), 1), new ChunkConnector(Vector2Int.down, new Vector2Int(4, 0), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 2, "The rotations length was " + rot.Length);
        Assert.Contains(0f, rot);
        Assert.Contains(270f, rot);
    }

    /// <summary>
    /// Tests if 2 Blocks with different Connectors can be aligned near each other.
    /// The result should be true (the operation is doable), and there should be 2 possible
    /// rotations to do that (0, 270)
    /// </summary>
    [Test]
    public void Test9()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 5), 1), new ChunkConnector(Vector2Int.right, new Vector2Int(9, 1), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.down, new Vector2Int(4, 0), 1), new ChunkConnector(Vector2Int.down, new Vector2Int(8, 0), 1), new ChunkConnector(Vector2Int.left, new Vector2Int(0, 5), 1), new ChunkConnector(Vector2Int.left, new Vector2Int(0, 1), 1), new ChunkConnector(Vector2Int.up, new Vector2Int(1, 9), 1), new ChunkConnector(Vector2Int.up, new Vector2Int(5, 9), 1), new ChunkConnector(Vector2Int.up, new Vector2Int(6, 9), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 2, "The rotations length was " + rot.Length);
        Assert.Contains(0f, rot);
        Assert.Contains(270f, rot);
    }
    /*
    [Test]
    public void Test9()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 5), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.up, new Vector2Int(5, 9), 1), new ChunkConnector(Vector2Int.up, new Vector2Int(6, 9), 1), new ChunkConnector(Vector2Int.left, new Vector2Int(0, 5), 1), new ChunkConnector(Vector2Int.left, new Vector2Int(0, 6), 1), new ChunkConnector(Vector2Int.down, new Vector2Int(4, 0), 1), new ChunkConnector(Vector2Int.down, new Vector2Int(5, 0), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.IsNull(rot);
    }

    /*
    [Test]
    public void Test10()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.right, new Vector2Int(9, 5), 1), new ChunkConnector(Vector2Int.right, new Vector2Int(9,1),1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.down, new Vector2Int(8, 0), 1), new ChunkConnector(Vector2Int.down, new Vector2Int(4, 0), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 1, "The rotations length was " + rot.Length);
        Assert.Contains(270f, rot);
    }

    [Test]
    public void Test00()
    {
        ChunkData chunk1 = GenerateChunkData();
        ChunkData chunk2 = GenerateChunkData();

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 4, "The rotations length was " + rot.Length);
    }


    [Test]
    public void Test001()
    {
        ChunkData chunk1 = GenerateChunkData();
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.left, new Vector2Int(0, 5), 1), new ChunkConnector(Vector2Int.right, new Vector2Int(9, 5), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.right, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 2, "The rotations length was " + rot.Length);
        Assert.Contains(90, rot);
        Assert.Contains(270, rot);
    }

    [Test]
    public void Test002()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(Vector2Int.left, new Vector2Int(0, 4), 1), new ChunkConnector(Vector2Int.right, new Vector2Int(9, 4), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(Vector2Int.left, new Vector2Int(0, 4), 1), new ChunkConnector(Vector2Int.up, new Vector2Int(5, 9), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, Vector2Int.up, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 2, "The rotations length was " + rot.Length);
        Assert.Contains(0, rot);
        Assert.Contains(270, rot);
    }
    /*
    [Test]
    public void Test11()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(ConnectorFace.RIGHT, new Vector2Int(9, 5), 1), new ChunkConnector(ConnectorFace.RIGHT, new Vector2Int(9, 1), 1));
        ChunkData chunk2 = GenerateChunkData();

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, ConnectorFace.RIGHT, ref rot);

        Assert.IsNull(rot);
    }

    [Test]
    public void Test12()
    {
        ChunkData chunk1 = GenerateChunkData(new ChunkConnector(ConnectorFace.RIGHT, new Vector2Int(9, 5), 1), new ChunkConnector(ConnectorFace.RIGHT, new Vector2Int(9, 1), 1));
        ChunkData chunk2 = GenerateChunkData(new ChunkConnector(ConnectorFace.DOWN, new Vector2Int(4,0), 1), new ChunkConnector(ConnectorFace.DOWN, new Vector2Int(8, 0), 1), new ChunkConnector(ConnectorFace.LEFT, new Vector2Int(0,5),1), new ChunkConnector(ConnectorFace.LEFT, new Vector2Int(0, 1), 1), new ChunkConnector(ConnectorFace.UP, new Vector2Int(1, 9), 1), new ChunkConnector(ConnectorFace.UP, new Vector2Int(5, 9), 1), new ChunkConnector(ConnectorFace.UP, new Vector2Int(6, 9), 1));

        float[] rot = null;
        TGAUtils.CheckChunkCompat(chunk1, chunk2, ConnectorFace.RIGHT, ref rot);

        Assert.AreEqual(true, rot != null, "The rotations were null");
        Assert.AreEqual(true, rot.Length == 2, "The rotations length was " + rot.Length);
        Assert.Contains(0f, rot);
        Assert.Contains(270f, rot);
    }
    */
}
