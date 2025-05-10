using System;
using UnityEngine;

namespace JRPGNavAgent2D
{


    public class NavAgent2D : MonoBehaviour
    {

        public NavMesh2D NavMesh2D;
        public float Speed = 3f;
        public bool UseAcceleration = true;
        public float Acceleration = 6f;
        public bool CanMoveDiagnally;
        public bool ShowGizmos = false;
        public bool IsStopped => moving == false;

        float speed;
        bool moving;
        Vector2[] path;
        int currentPathNodeIndex;

        void Start()
        {
            if (NavMesh2D == null)
            {
                NavMesh2D = GameObject.FindAnyObjectByType<NavMesh2D>();
            }

            if (NavMesh2D == null)
            {
                Debug.LogError("Missing NavMesh2D!");
            }
        }

        public bool Move(Vector2 position)
        {
            var newPath = NavMesh2D.CreatePath(transform.position, position, CanMoveDiagnally);

            if (newPath == null)
            {
                return false;
            }

            if (newPath.Length != 0)
            {
                StartMoving(newPath);
            }

            return true;
        }

        public bool MoveToTouch(Vector2 position, int tilesAway = 1)
        {
            var newPath = NavMesh2D.CreatePath(transform.position, position, CanMoveDiagnally);

            if (newPath == null)
            {
                return false;
            }

            if (newPath.Length > tilesAway)
            {
                Array.Resize(ref newPath, newPath.Length - tilesAway);

                StartMoving(newPath);
            }

            return true;
        }

        void StartMoving(Vector2[] path)
        {
            this.path = path;

            moving = true;

            if (UseAcceleration == false)
            {
                speed = Speed;
            }

            currentPathNodeIndex = 0;
        }

        public void Stop()
        {
            moving = false;
            path = null;
        }

        void Update()
        {
            if (moving)
            {
                if (UseAcceleration)
                {
                    speed = Mathf.Min((Time.deltaTime * Acceleration) + speed, Speed);
                }

                float moveDistance = speed * Time.deltaTime;

                Vector2 position = transform.position;

                while (moveDistance > 0)
                {
                    Vector2 dist = path[currentPathNodeIndex] - new Vector2(transform.position.x, transform.position.y);

                    if (dist.magnitude < moveDistance)
                    {
                        moveDistance -= dist.magnitude;
                        position = path[currentPathNodeIndex];
                        currentPathNodeIndex += 1;

                        if (currentPathNodeIndex == path.Length)
                        {
                            moving = false;
                            speed = 0;
                            break;
                        }
                    }
                    else
                    {
                        var dir = (path[currentPathNodeIndex] - position).normalized;
                        position += dir * moveDistance;
                        moveDistance = 0;
                    }
                }

                transform.position = position;
            }
        }

        void OnDrawGizmos()
        {
            if (ShowGizmos && moving)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawLine(transform.position, path[currentPathNodeIndex]);

                for (int i = currentPathNodeIndex; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
    }
}