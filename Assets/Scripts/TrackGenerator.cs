using UnityEngine;

/// <summary>
/// Генератор полоси: створює сегменти дороги, перешкоди, ями та фініш.
/// </summary>
public class TrackGenerator : MonoBehaviour
{
    [Header("Параметри полоси")]
    public float trackWidth = 9f;
    public float segmentLength = 20f;
    public int totalSegments = 30;

    [Header("Перешкоди та об'єкти")]
    public float obstacleChance = 0.3f;
    public float pitChance = 0.15f;
    public float coinChance = 0.5f;
    public float trapChance = 0.2f;
    public float laneDistance = 3f;

    [Header("Матеріали")]
    public Material roadMaterial;
    public Material obstacleMaterial;
    public Material pitMaterial;
    public Material finishMaterial;

    void Start()
    {
        GenerateTrack();
    }

    void GenerateTrack()
    {
        for (int i = 0; i < totalSegments; i++)
        {
            float zPos = i * segmentLength;

            // Створення сегменту дороги
            if (Random.value < pitChance && i > 2 && i < totalSegments - 2)
            {
                CreatePit(zPos);
            }
            else
            {
                CreateRoadSegment(zPos);

                // Додавання перешкод (не на першому та останньому сегменті)
                if (i > 1 && i < totalSegments - 1 && Random.value < obstacleChance)
                {
                    CreateObstacle(zPos);
                }

                // Монети
                if (i > 0 && i < totalSegments - 1 && Random.value < coinChance)
                {
                    CreateCoin(zPos);
                }

                // Пастки
                if (i > 2 && i < totalSegments - 1 && Random.value < trapChance)
                {
                    CreateTrap(zPos);
                }
            }
        }

        // Фініш на останньому сегменті
        CreateFinish((totalSegments - 1) * segmentLength);
    }

    void CreateRoadSegment(float zPosition)
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.name = "Road";
        road.transform.position = new Vector3(0f, -0.5f, zPosition + segmentLength / 2f);
        road.transform.localScale = new Vector3(trackWidth, 1f, segmentLength);
        road.transform.parent = transform;

        if (roadMaterial != null)
            road.GetComponent<Renderer>().material = roadMaterial;
        else
            road.GetComponent<Renderer>().material.color = Color.gray;
    }

    void CreateObstacle(float zPosition)
    {
        // Випадкова полоса для перешкоди
        int lane = Random.Range(-1, 2);
        float xPos = lane * laneDistance;

        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = "Obstacle";
        obstacle.tag = "Obstacle";
        obstacle.transform.position = new Vector3(xPos, 0.75f, zPosition + segmentLength / 2f);
        obstacle.transform.localScale = new Vector3(2.5f, 1.5f, 1f);
        obstacle.transform.parent = transform;

        if (obstacleMaterial != null)
            obstacle.GetComponent<Renderer>().material = obstacleMaterial;
        else
            obstacle.GetComponent<Renderer>().material.color = Color.red;
    }

    void CreatePit(float zPosition)
    {
        // Яма — тригер під рівнем дороги
        GameObject pit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pit.name = "Pit";
        pit.tag = "Pit";
        pit.transform.position = new Vector3(0f, -2f, zPosition + segmentLength / 2f);
        pit.transform.localScale = new Vector3(trackWidth, 1f, segmentLength * 0.8f);
        pit.transform.parent = transform;

        // Зробити тригером
        pit.GetComponent<Collider>().isTrigger = true;

        if (pitMaterial != null)
            pit.GetComponent<Renderer>().material = pitMaterial;
        else
            pit.GetComponent<Renderer>().material.color = new Color(0.2f, 0.1f, 0f);

        // Візуальні краї ями (зменшені сегменти дороги зліва і справа — опціонально)
        // Короткі ділянки дороги до і після ями
        GameObject edgeBefore = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edgeBefore.name = "PitEdge";
        edgeBefore.transform.position = new Vector3(0f, -0.5f, zPosition);
        edgeBefore.transform.localScale = new Vector3(trackWidth, 1f, segmentLength * 0.1f);
        edgeBefore.transform.parent = transform;
        edgeBefore.GetComponent<Renderer>().material.color = Color.gray;

        GameObject edgeAfter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edgeAfter.name = "PitEdge";
        edgeAfter.transform.position = new Vector3(0f, -0.5f, zPosition + segmentLength);
        edgeAfter.transform.localScale = new Vector3(trackWidth, 1f, segmentLength * 0.1f);
        edgeAfter.transform.parent = transform;
        edgeAfter.GetComponent<Renderer>().material.color = Color.gray;
    }

    void CreateCoin(float zPosition)
    {
        int lane = Random.Range(-1, 2);
        float xPos = lane * laneDistance;

        GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coin.name = "Coin";
        coin.tag = "Coin";
        coin.transform.position = new Vector3(xPos, 1.2f, zPosition + segmentLength * 0.7f);
        coin.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        coin.transform.parent = transform;

        coin.GetComponent<Collider>().isTrigger = true;
        coin.GetComponent<Renderer>().material.color = Color.yellow;
    }

    void CreateTrap(float zPosition)
    {
        int lane = Random.Range(-1, 2);
        float xPos = lane * laneDistance;

        GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trap.name = "Trap";
        trap.tag = "Trap";
        trap.transform.position = new Vector3(xPos, 0.15f, zPosition + segmentLength * 0.3f);
        trap.transform.localScale = new Vector3(1.5f, 0.15f, 1.5f);
        trap.transform.parent = transform;

        trap.GetComponent<Collider>().isTrigger = true;
        trap.GetComponent<Renderer>().material.color = new Color(0.8f, 0f, 0.8f); // фіолетовий
    }

    void CreateFinish(float zPosition)
    {
        GameObject finish = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finish.name = "Finish";
        finish.tag = "Finish";
        finish.transform.position = new Vector3(0f, 1.5f, zPosition);
        finish.transform.localScale = new Vector3(trackWidth, 3f, 0.5f);
        finish.transform.parent = transform;

        finish.GetComponent<Collider>().isTrigger = true;

        if (finishMaterial != null)
            finish.GetComponent<Renderer>().material = finishMaterial;
        else
        {
            Material mat = finish.GetComponent<Renderer>().material;
            mat.color = Color.green;
        }
    }
}
