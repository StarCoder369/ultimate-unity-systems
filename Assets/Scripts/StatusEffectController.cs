using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    [System.Serializable]
    public class ActiveStatus
    {
        public StatusData data;

        public GameObject activeStatusImg;
        public StatusEffectImgHandler activeStatusImgHandler;

        public float buildUp;
        public float remainingTime;

        public bool active;

        public GameObject effectObject;
    }


    public List<ActiveStatus> activeStatuses = new List<ActiveStatus>();

    public GameObject statusImg;
    public Canvas statusHolderCanvas;


    private void Update()
    {
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            ActiveStatus status = activeStatuses[i];


            if (status.active)
            {
                status.remainingTime -= Time.deltaTime * status.data.activeDecay;


                for (int j = 0; j < status.data.effects.Count; j++)
                {
                    status.data.effects[j].OnTick(gameObject);
                }


                if (status.remainingTime <= 0f)
                {
                    RemoveStatus(status.data);
                    continue;
                }
            }
            else
            {
                status.buildUp -= status.data.buildUpDecay * Time.deltaTime;

                status.buildUp = Mathf.Max(status.buildUp, 0f);
            }


            if (status.activeStatusImgHandler != null)
            {
                status.activeStatusImgHandler.fillProgress = GetStatusProgress(status.data);
            }
        }
    }


    public void ApplyStatus(StatusData status)
    {
        ApplyStatusWithoutDefault(status, Mathf.RoundToInt(status.defaultBuildUpPerHit));
    }


    public void ApplyStatusWithoutDefault(StatusData status, int buildUp)
    {
        ActiveStatus existingStatus = GetStatus(status);


        if (existingStatus == null)
        {
            existingStatus = new ActiveStatus();

            existingStatus.data = status;

            activeStatuses.Add(existingStatus);


            if (statusImg != null && statusHolderCanvas != null)
            {
                existingStatus.activeStatusImg = Instantiate(statusImg, statusHolderCanvas.transform);

                existingStatus.activeStatusImgHandler = existingStatus.activeStatusImg.GetComponent<StatusEffectImgHandler>();

                if (existingStatus.activeStatusImgHandler != null)
                {
                    existingStatus.activeStatusImgHandler.statusData = status;
                }
            }
        }


        if (existingStatus.active)
        {
            return;
        }


        existingStatus.buildUp += buildUp;


        if (existingStatus.buildUp >= status.maxBuildUp)
        {
            ActivateStatus(existingStatus);
        }
    }


    private void ActivateStatus(ActiveStatus status)
    {
        status.active = true;

        status.remainingTime = status.data.duration;

        status.buildUp = 0f;


        if (status.data.effectPrefab != null)
        {
            status.effectObject = Instantiate(status.data.effectPrefab, transform);
        }


        for (int i = 0; i < status.data.effects.Count; i++)
        {
            status.data.effects[i].OnStart(gameObject);
        }
    }


    public void RemoveStatus(StatusData status)
    {
        ActiveStatus activeStatus = GetStatus(status);


        if (activeStatus == null)
        {
            return;
        }


        for (int i = 0; i < activeStatus.data.effects.Count; i++)
        {
            activeStatus.data.effects[i].OnEnd(gameObject);
        }


        if (activeStatus.effectObject != null)
        {
            Destroy(activeStatus.effectObject);
        }


        if (activeStatus.activeStatusImg != null)
        {
            Destroy(activeStatus.activeStatusImg);
        }


        activeStatuses.Remove(activeStatus);
    }


    public bool HasStatus(StatusData status)
    {
        return GetStatus(status) != null;
    }


    public float GetStatusProgress(StatusData status)
    {
        ActiveStatus activeStatus = GetStatus(status);


        if (activeStatus == null)
        {
            return 0f;
        }


        if (activeStatus.active)
        {
            return activeStatus.remainingTime / activeStatus.data.duration;
        }


        return activeStatus.buildUp / activeStatus.data.maxBuildUp;
    }


    public List<ActiveStatus> GetActiveStatuses()
    {
        return activeStatuses;
    }


    private ActiveStatus GetStatus(StatusData status)
    {
        for (int i = 0; i < activeStatuses.Count; i++)
        {
            if (activeStatuses[i].data == status)
            {
                return activeStatuses[i];
            }
        }


        return null;
    }
}