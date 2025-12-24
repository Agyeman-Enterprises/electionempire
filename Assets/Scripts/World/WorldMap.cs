using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.World
{
    /// <summary>
    /// Interactive map visualization for the world
    /// </summary>
    public class WorldMap : MonoBehaviour
    {
        [Header("World Reference")]
        public World World;
        
        [Header("Camera")]
        public Camera MapCamera;
        
        [Header("Visual Prefabs")]
        public GameObject RegionPrefab;
        public GameObject StatePrefab;
        public GameObject DistrictPrefab;
        
        [Header("Map Settings")]
        public float GridSize = 30f; // Districts per row
        public float DistrictSpacing = 2f;
        
        private Dictionary<string, GameObject> districtObjects;
        private Dictionary<string, Color> playerColors;
        
        private void Start()
        {
            districtObjects = new Dictionary<string, GameObject>();
            playerColors = new Dictionary<string, Color>();
            
            if (World != null)
            {
                GenerateMap();
            }
        }
        
        public void GenerateMap()
        {
            if (World == null || World.Nation == null)
            {
                Debug.LogError("World not set!");
                return;
            }
            
            ClearMap();
            
            int districtIndex = 0;
            
            foreach (var region in World.Nation.Regions)
            {
                foreach (var state in region.States)
                {
                    foreach (var district in state.Districts)
                    {
                        // Calculate grid position
                        float x = (districtIndex % GridSize) * DistrictSpacing;
                        float y = Mathf.Floor(districtIndex / GridSize) * DistrictSpacing;
                        
                        // Create district visual
                        GameObject districtObj;
                        
                        if (DistrictPrefab != null)
                        {
                            districtObj = Instantiate(DistrictPrefab, 
                                new Vector3(x, y, 0), Quaternion.identity, transform);
                        }
                        else
                        {
                            // Fallback: create simple cube
                            districtObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            districtObj.transform.position = new Vector3(x, y, 0);
                            districtObj.transform.SetParent(transform);
                        }
                        
                        districtObj.name = district.Name;
                        
                        // Store reference
                        districtObjects[district.ID] = districtObj;
                        
                        // Color by type
                        var renderer = districtObj.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.material.color = GetDistrictTypeColor(district.Type);
                        }
                        
                        // Add district component
                        var districtComp = districtObj.GetComponent<DistrictMapElement>();
                        if (districtComp == null)
                            districtComp = districtObj.AddComponent<DistrictMapElement>();
                        districtComp.District = district;
                        
                        districtIndex++;
                    }
                }
            }
            
            // Center camera
            if (MapCamera != null)
            {
                float centerX = (GridSize * DistrictSpacing) / 2f;
                float centerY = (districtIndex / GridSize * DistrictSpacing) / 2f;
                MapCamera.transform.position = new Vector3(centerX, centerY, -10);
            }
        }
        
        private Color GetDistrictTypeColor(DistrictType type)
        {
            switch (type)
            {
                case DistrictType.Urban: 
                    return new Color(0.2f, 0.2f, 0.2f); // Dark gray
                case DistrictType.Suburban: 
                    return new Color(0.6f, 0.6f, 0.6f); // Light gray
                case DistrictType.Rural: 
                    return new Color(0.3f, 0.6f, 0.3f); // Green
                default: 
                    return Color.white;
            }
        }
        
        /// <summary>
        /// Color districts by polling
        /// </summary>
        public void ColorByPolling(PlayerState player, VoterSimulation simulation)
        {
            if (player == null || simulation == null) return;
            
            foreach (var region in World.Nation.Regions)
            {
                foreach (var state in region.States)
                {
                    foreach (var district in state.Districts)
                    {
                        float polling = simulation.CalculateDistrictPolling(player, district);
                        
                        if (districtObjects.ContainsKey(district.ID))
                        {
                            var obj = districtObjects[district.ID];
                            var renderer = obj.GetComponent<Renderer>();
                            
                            if (renderer != null)
                            {
                                // Gradient from red (0%) to green (100%)
                                renderer.material.color = Color.Lerp(Color.red, Color.green, polling / 100f);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Color districts by election results
        /// </summary>
        public void ColorByResults(Dictionary<string, string> results)
        {
            foreach (var kvp in results)
            {
                string districtID = kvp.Key;
                string winner = kvp.Value;
                
                if (districtObjects.ContainsKey(districtID))
                {
                    var obj = districtObjects[districtID];
                    var renderer = obj.GetComponent<Renderer>();
                    
                    if (renderer != null)
                    {
                        // Assign color by winner (different color per player)
                        renderer.material.color = GetPlayerColor(winner);
                    }
                }
            }
        }
        
        private Color GetPlayerColor(string playerName)
        {
            if (!playerColors.ContainsKey(playerName))
            {
                // Generate random color for this player
                playerColors[playerName] = new Color(
                    UnityEngine.Random.Range(0.3f, 1f),
                    UnityEngine.Random.Range(0.3f, 1f),
                    UnityEngine.Random.Range(0.3f, 1f)
                );
            }
            
            return playerColors[playerName];
        }
        
        /// <summary>
        /// Reset map to default colors
        /// </summary>
        public void ResetColors()
        {
            foreach (var region in World.Nation.Regions)
            {
                foreach (var state in region.States)
                {
                    foreach (var district in state.Districts)
                    {
                        if (districtObjects.ContainsKey(district.ID))
                        {
                            var obj = districtObjects[district.ID];
                            var renderer = obj.GetComponent<Renderer>();
                            
                            if (renderer != null)
                            {
                                renderer.material.color = GetDistrictTypeColor(district.Type);
                            }
                        }
                    }
                }
            }
        }
        
        private void ClearMap()
        {
            foreach (var obj in districtObjects.Values)
            {
                if (obj != null)
                    Destroy(obj);
            }
            
            districtObjects.Clear();
        }
        
        // Zoom and pan controls
        private void Update()
        {
            if (MapCamera == null) return;
            
            // Mouse scroll to zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                if (MapCamera.orthographic)
                {
                    MapCamera.orthographicSize -= scroll * 5f;
                    MapCamera.orthographicSize = Mathf.Clamp(MapCamera.orthographicSize, 5f, 50f);
                }
                else
                {
                    MapCamera.fieldOfView -= scroll * 10f;
                    MapCamera.fieldOfView = Mathf.Clamp(MapCamera.fieldOfView, 20f, 90f);
                }
            }
            
            // Middle mouse drag to pan
            if (Input.GetMouseButton(2))
            {
                float moveX = -Input.GetAxis("Mouse X") * 0.5f;
                float moveY = -Input.GetAxis("Mouse Y") * 0.5f;
                MapCamera.transform.Translate(moveX, moveY, 0);
            }
            
            // WASD or Arrow keys to pan
            float panX = Input.GetAxis("Horizontal");
            float panY = Input.GetAxis("Vertical");
            if (panX != 0 || panY != 0)
            {
                MapCamera.transform.Translate(new Vector3(panX, panY, 0) * Time.deltaTime * 10f);
            }
        }
    }
}

