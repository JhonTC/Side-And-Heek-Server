using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayRecorder
{
    public enum ObjectEvent
    {
        Spawned,
        Destoryed
    }

    public enum RecorderState
    {
        Stopped,
        Recording,
        Playing
    }

    public class ObjectRecord
    {
        public Transform objectRecorded;
        public int startingFrame;
        public Queue<Vector3> positions;
        public Queue<Quaternion> rotations;
        public Queue<Vector3> scales;

        public ObjectRecord(Transform _objectRecorded, int _startingFrame)
        {
            objectRecorded = _objectRecorded;
            startingFrame = _startingFrame;
            positions = new Queue<Vector3>();
            rotations = new Queue<Quaternion>();
            scales = new Queue<Vector3>();
        }

        public virtual void AddFrame()
        {
            positions.Enqueue(objectRecorded.position);
            rotations.Enqueue(objectRecorded.rotation);
            scales.Enqueue(objectRecorded.localScale);
        }

        public virtual void RemoveFrame()
        {
            positions.Dequeue();
            rotations.Dequeue();
            scales.Dequeue();
        }

        public virtual void PlayNextFrame()
        {
            objectRecorded.position = positions.Dequeue();
            objectRecorded.rotation = rotations.Dequeue();
            objectRecorded.localScale = scales.Dequeue();
        }
    }

    public class PlayerObjectRecord : ObjectRecord
    {
        public List<ObjectRecord> limbs;

        public PlayerObjectRecord(Player player, int _startingFrame) : base(player.movementController.root.transform, _startingFrame)
        {
            limbs = new List<ObjectRecord>();

            limbs.Add(new ObjectRecord(player.movementController.rightLeg.transform, _startingFrame));
            limbs.Add(new ObjectRecord(player.movementController.leftLeg.transform, _startingFrame));
            limbs.Add(new ObjectRecord(player.movementController.rightFootCollider.foot.transform, _startingFrame));
            limbs.Add(new ObjectRecord(player.movementController.leftFootCollider.foot.transform, _startingFrame));
        }

        public override void AddFrame()
        {
            base.AddFrame();

            foreach (ObjectRecord objectRecord in limbs)
            {
                objectRecord.AddFrame();
            }
        }

        public override void RemoveFrame()
        {
            base.RemoveFrame();

            foreach (ObjectRecord objectRecord in limbs)
            {
                objectRecord.RemoveFrame();
            }
        }

        public override void PlayNextFrame()
        {
            base.PlayNextFrame();

            foreach (ObjectRecord objectRecord in limbs)
            {
                objectRecord.PlayNextFrame();
            }
        }
    }

    private Dictionary<string, ObjectRecord> objectRecords = new Dictionary<string, ObjectRecord>();
    private int currentFrame = 0;
    private int targetFrame = 0;
    private int deleteframe = 0;

    private int recordLength = 10; //in seconds

    private RecorderState recorderState = RecorderState.Stopped;

    public void FixedUpdate()
    {
        if (recorderState == RecorderState.Recording)
        {
            foreach (Player player in Player.list.Values)
            {
                string id = "Player" + player.Id.ToString();

                if (!objectRecords.ContainsKey(id))
                {
                    objectRecords.Add(id, new PlayerObjectRecord(player, currentFrame));
                }

                objectRecords[id].AddFrame();
            }

            foreach (NetworkObject networkObject in NetworkObjectsManager.networkObjects.Values)
            {
                string id = "NetworkObject" + networkObject.objectId.ToString();

                if (!objectRecords.ContainsKey(id))
                {
                    objectRecords.Add(id, new ObjectRecord(networkObject.transform, currentFrame));
                }

                objectRecords[id].AddFrame();
            }

            currentFrame++;

            float durationInSeconds = currentFrame * Time.fixedDeltaTime;
            if (durationInSeconds >= recordLength)
            {
                foreach (ObjectRecord objectRecord in objectRecords.Values)
                {
                    if (objectRecord.startingFrame <= deleteframe)
                    {
                        objectRecord.RemoveFrame();
                    }
                }

                deleteframe++;
            }
        }

        if (recorderState == RecorderState.Playing)
        {
            foreach (ObjectRecord objectRecord in objectRecords.Values)
            {
                if (currentFrame >= objectRecord.startingFrame)
                {
                    objectRecord.PlayNextFrame();
                }
            }

            currentFrame++;
            if (currentFrame >= targetFrame)
            {
                currentFrame = 0;
                StopRecording();
            }
        }
    }

    public void StartRecording()
    {
        objectRecords.Clear();

        currentFrame = 0;
        deleteframe = 0;
        recorderState = RecorderState.Recording;
    }

    public void StopRecording()
    {
        targetFrame = currentFrame - deleteframe;
        recorderState = RecorderState.Stopped;
    }

    public void PlayRecording()
    {
        if (recorderState == RecorderState.Recording)
        {
            StopRecording();
        }

        currentFrame = 0;
        recorderState = RecorderState.Playing;
    }
}
