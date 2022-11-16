using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SVision : MonoBehaviour
{
    public Vector2Int startPos;
    public float maxDotProduct;
    public int visionRadius;
    public int smellRadius;
    private List<Partition> visiblePartitions = new List<Partition>();
    private List<Partition> smelledPartitions = new List<Partition>();
    private PartitionSystem pSystem;
    private Vector3 oldForwardVector;
    private Vector3 oldPos;
    public enum channel{sight, smell, sound, none};
    public channel SVChannel = channel.none;
    void Start()
    {
        pSystem = PartitionSystem.instance;
        Vector3 pos = pSystem.PartitionToWorldCoords(startPos);
        transform.position = pos + Vector3.up;
    }

    void Update()
    {
        if(oldForwardVector != transform.forward || oldPos != transform.position)
        {
            visiblePartitions = CalculateVision();
            smelledPartitions = CalculateSmell();
            oldForwardVector = transform.forward;
            oldPos = transform.position;
        }
    }
    private List<Partition> CalculateVision()
    {
        //Get all partitions in radius
        List<Partition> partitions = new List<Partition>();
        partitions.AddRange(pSystem.GetPartitionsInRadius(transform.position,visionRadius));

        //Cull partitions that dont have the min dot product
        List<Partition> partitionsToCull = new List<Partition>();
        foreach(Partition p in partitions)
        {
            Vector3 pos = transform.position - pSystem.PartitionToWorldCoords(p.coords);
            pos.Normalize();
            float dot = Vector3.Dot(transform.forward, pos);
            if(dot > maxDotProduct)
            {
                partitionsToCull.Add(p);
            }
            else if(Vector3.Distance(transform.position, pSystem.PartitionToWorldCoords(p.coords)) > visionRadius)
            {
                partitionsToCull.Add(p);
            }
        }
        foreach(Partition p in partitionsToCull)
        {
            partitions.Remove(p);
        }
        Vector2Int localPartition = pSystem.WorldToPartitionCoords(transform.position);
        partitions.Add(pSystem.partitions[localPartition.x,localPartition.y]);
        return partitions;
    }
    private List<Partition> CalculateSmell()
    {
        List<Partition> nearbyPartitions = new List<Partition>();
        nearbyPartitions.AddRange(pSystem.GetPartitionsInRadius(transform.position, smellRadius));
        List<Partition> hasSmell = new List<Partition>();
        foreach(Partition p in nearbyPartitions)
        {
            if(p.HasFood() || p.agents.Count > 0)
            {
                hasSmell.Add(p);
            }
        }
        return hasSmell;
    }
    void OnDrawGizmos()
    {
        if(visiblePartitions != null && pSystem != null)
        {
            switch(SVChannel)
            {
                case(channel.sight):
                    Gizmos.color = Color.white;
                    foreach(Partition p in visiblePartitions)
                    {
                        Vector3 pos = pSystem.PartitionToWorldCoords(p.coords);
                        Gizmos.DrawWireCube(pos + Vector3.up, Vector3.one);
                    }
                    break;
                case(channel.smell):
                    Gizmos.color = Color.green;
                    foreach(Partition p in smelledPartitions)
                    {
                        Vector3 pos = pSystem.PartitionToWorldCoords(p.coords);
                        Gizmos.DrawWireCube(pos + Vector3.up, Vector3.one);
                    }
                    break;
                case(channel.sound):
                    Gizmos.color = Color.blue;
                    break;
                default:
                    break;
            }

        }
    }
}
