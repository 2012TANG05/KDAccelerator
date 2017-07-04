using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculateModelClasses
{
    public class KDNode
    {
        public List<Triangle> primitives;//包含三角面
        public KDNode left;//左子节点
        public KDNode right;//右子节点
        public Bounds3 box;//包围盒
        public double splitPlane;//分割面
        public int flag;//区分基于x,y,z轴划分的内部节点（对应0,1,2）以及叶节点（对应3）

        public void InitLeaf(List<Triangle> triangles)
        {
            primitives = new List<Triangle>(triangles);//传值传引用可能会出问题，mark
        }

        public void InitInterior(int axis, int aboveChild, float splitPlane)
        {
            this.splitPlane = splitPlane;
            this.flag = axis;
        }

        public bool IsLeaf()
        {
            return flag == 3 ? true : false;
        }

        public int SplitAxis()
        {
            return flag;
        }
    }

    /// <summary>
    /// 记录包围盒轴向上边的位置
    /// </summary>
    public struct BoundEdge
    {
        public double t;
        public int primNum;
        public enum EdgeType { Start, End };
        public EdgeType type;
        public BoundEdge(double t, int primNum, bool starting)
        {
            this.t = t;
            this.primNum = primNum;
            this.type = starting ? EdgeType.Start : EdgeType.End;
        }
        public static bool operator <(BoundEdge e1, BoundEdge e2)//结构体排序会用
        {
            if (e1.t == e2.t)
                return (int)e1.type < (int)e2.type;
            else
                return e1.t < e2.t;
        }

        public static bool operator >(BoundEdge e1, BoundEdge e2)
        {
            if (e1.t == e2.t)
                return (int)e1.type > (int)e2.type;
            else
                return e1.t > e2.t;
        }
    }

    public class KDTreeAccelerator
    {
        private int isectCost, traversalCost, maxPrims;
        private double emptyBonus;
        private List<Triangle> primitives = new List<Triangle>();
        private List<KDNode> nodes = new List<KDNode>();
        private int nAllocedNodes, nextFreeNode;//记录已分配的节点数量 和 数组中下一个有效节点
        private Bounds3 bounds;

        public KDTreeAccelerator(List<Triangle> primitives, int isectCost = 80, int traversalCost = 1, 
            double emptyBonus = 0.5, int maxPrims = 1, int maxDepth = -1)
        {
            this.isectCost = isectCost;
            this.traversalCost = traversalCost;
            this.maxPrims = maxPrims;
            this.emptyBonus = emptyBonus;
            this.primitives = primitives;

            //Build kd-Tree for accelerator
            nextFreeNode = nAllocedNodes = 0;
            if (maxDepth <= 0)
                maxDepth = (int)Math.Round(8 + 1.3 * (Math.Log(primitives.Count) / Math.Log(2)),0);

            //Compute bounds for kd-tree construction
            List<Bounds3> primBounds = new List<Bounds3>();
            for (int i = 0; i < primitives.Count; i++)
            {
                Bounds3 tempBound = primitives[i].WorldBound();
                bounds = bounds.Union(tempBound);
                primBounds.Add(tempBound);
            }

            // Allocate working memory for kd-tree construction C#中这一步已简化
            List<List<BoundEdge>> edges = new List<List<BoundEdge>>();
            int[] prims0 = new int[primitives.Count];
            int[] prims1 = new int[(maxDepth+1)*primitives.Count];

            // Initialize _primNums_ for kd-tree construction
            int[] primNums = new int[primitives.Count];
            for (int i = 0; i < primitives.Count; i++)
            {
                primNums[i] = i;
            }

            // Start recursive construction of kd-tree
            
        }

        private void BuildTree(int nodeNum, ref Bounds3 nodeBounds, ref List<Bounds3> allPrimBounds, 
            int[] primNums, List<Triangle> tris, int depth, List<List<BoundEdge>> edges, 
            int[] prims0, int[] prims1, int badRefines)
        {
            // Get next free node from _nodes_ array

            // Initialize leaf node if termination criteria met
            if (tris.Count <= maxPrims || depth == 0)
            {
                nodes[nodeNum].InitLeaf(tris);
                return;
            }

        // Initialize interior node and continue recursion 初始化内部节点并递归创建

            // Choose split axis position for interior node 为内部节点选择分割轴位置
            int bestAxis = -1, bestOffset = -1;
            double bestCost = Double.PositiveInfinity;
            double oldCost = isectCost*(double)(tris.Count);//与所有面判交成本
            double totalSA = nodeBounds.SurfaceArea();//当前包围盒表面积
            double invTotalSA = 1.0 / totalSA;
            SpectVector d = new SpectVector(nodeBounds.pMin, nodeBounds.pMax);
            // Choose which axis to split along
            int axis = nodeBounds.MaximumExtent();
            int retries = 0;
        retrySplit:
            // Initialize edges for _axis_
            List<BoundEdge> bes = new List<BoundEdge>();
            for (int i = 0; i < tris.Count; i++)
            {
                int pn = primNums[i];
                Bounds3 bbox = allPrimBounds[pn];
                if (axis == 0)
                {
                    edges[axis][2 * i] = new BoundEdge(bbox.pMin.X, pn, true);
                    bes.Add(edges[axis][2 * i]);
                    edges[axis][2 * i + 1] = new BoundEdge(bbox.pMax.X, pn, false);
                    bes.Add(edges[axis][2 * i + 1]);
                }
                else if (axis == 1)
                {
                    edges[axis][2 * i] = new BoundEdge(bbox.pMin.Y, pn, true);
                    bes.Add(edges[axis][2 * i]);
                    edges[axis][2 * i + 1] = new BoundEdge(bbox.pMax.Y, pn, false);
                    bes.Add(edges[axis][2 * i + 1]);
                }
                else if (axis == 2)
                {
                    edges[axis][2 * i] = new BoundEdge(bbox.pMin.Z, pn, true);
                    bes.Add(edges[axis][2 * i]);
                    edges[axis][2 * i + 1] = new BoundEdge(bbox.pMax.Z, pn, false);
                    bes.Add(edges[axis][2 * i + 1]);
                }
                else
                    throw new Exception { };
            }
            bes.Sort();//对包围盒在轴线的投影进行排序
            for (int i = 0; i < tris.Count; i++)
            {
                edges[axis][2 * i] = bes[i];
                edges[axis][2 * i + 1] = bes[2 * i + 1];
            }

            // Compute cost of all splits for _axis_ to find best
            int nBelow = 0, nAbove = tris.Count;
            for (int i = 0; i < 2 * tris.Count; i++)
            {
                if (edges[axis][i].type == BoundEdge.EdgeType.End) --nAbove;
                double edget = edges[axis][i].t;
                switch (axis)
                {
                    case 0:
                        if (edget > nodeBounds.pMin.X && edget < nodeBounds.pMax.X)
                        {
                            // Compute cost for split at _i_th edge 计算当前分割轴的开销
                            double belowSA = 2 * ((nodeBounds.pMax.Y - nodeBounds.pMin.Y) * (nodeBounds.pMax.Z - nodeBounds.pMin.Z) +
                                (edget - nodeBounds.pMin.X) * ((nodeBounds.pMax.Y - nodeBounds.pMin.Y) + (nodeBounds.pMax.Z - nodeBounds.pMin.Z)));//分割面下方的表面积
                            double aboveSA = 2 * ((nodeBounds.pMax.Y - nodeBounds.pMin.Y) * (nodeBounds.pMax.Z - nodeBounds.pMin.Z) +
                                (nodeBounds.pMax.X - edget) * ((nodeBounds.pMax.Y - nodeBounds.pMin.Y) + (nodeBounds.pMax.Z - nodeBounds.pMin.Z)));//分割面上方的表面积
                            double pBelow = belowSA * invTotalSA;//射线穿过下包围盒的概率
                            double pAbove = aboveSA * invTotalSA;//射线穿过上包围盒的概率
                            double eb = (nAbove == 0 || nBelow == 0) ? emptyBonus : 0.0;//分割面为包围盒的一个面时，修正系数
                            double cost = traversalCost + isectCost * (1.0 - eb) * (pBelow * nBelow + pAbove * nAbove);//计算开销
                            // Update best split if this is lowest cost so far
                            if (cost < bestCost)
                            {
                                bestCost = cost;
                                bestAxis = axis;
                                bestOffset = i;
                            }
                        } break;
                    case 1:
                        if (edget > nodeBounds.pMin.Y && edget < nodeBounds.pMax.Y)
                        {
                            // Compute cost for split at _i_th edge 计算当前分割轴的开销
                            double belowSA = 2 * ((nodeBounds.pMax.X - nodeBounds.pMin.X) * (nodeBounds.pMax.Z - nodeBounds.pMin.Z) +
                                (edget - nodeBounds.pMin.Y) * ((nodeBounds.pMax.X - nodeBounds.pMin.X) + (nodeBounds.pMax.Z - nodeBounds.pMin.Z)));//分割面下方的表面积
                            double aboveSA = 2 * ((nodeBounds.pMax.X - nodeBounds.pMin.X) * (nodeBounds.pMax.Z - nodeBounds.pMin.Z) +
                                (nodeBounds.pMax.Y - edget) * ((nodeBounds.pMax.X - nodeBounds.pMin.X) + (nodeBounds.pMax.Z - nodeBounds.pMin.Z)));//分割面上方的表面积
                            double pBelow = belowSA * invTotalSA;//射线穿过下包围盒的概率
                            double pAbove = aboveSA * invTotalSA;//射线穿过上包围盒的概率
                            double eb = (nAbove == 0 || nBelow == 0) ? emptyBonus : 0.0;//分割面为包围盒的一个面时，修正系数
                            double cost = traversalCost + isectCost * (1.0 - eb) * (pBelow * nBelow + pAbove * nAbove);//计算开销
                            // Update best split if this is lowest cost so far
                            if (cost < bestCost)
                            {
                                bestCost = cost;
                                bestAxis = axis;
                                bestOffset = i;
                            }
                        } break;
                    case 2:
                        if (edget > nodeBounds.pMin.Z && edget < nodeBounds.pMax.Z)
                        {
                            // Compute cost for split at _i_th edge 计算当前分割轴的开销
                            double belowSA = 2 * ((nodeBounds.pMax.X - nodeBounds.pMin.X) * (nodeBounds.pMax.Y - nodeBounds.pMin.Y) +
                                (edget - nodeBounds.pMin.Z) * ((nodeBounds.pMax.X - nodeBounds.pMin.X) + (nodeBounds.pMax.Y - nodeBounds.pMin.Y)));//分割面下方的表面积
                            double aboveSA = 2 * ((nodeBounds.pMax.X - nodeBounds.pMin.X) * (nodeBounds.pMax.Y - nodeBounds.pMin.Y) +
                                (nodeBounds.pMax.Z - edget) * ((nodeBounds.pMax.X - nodeBounds.pMin.X) + (nodeBounds.pMax.Y - nodeBounds.pMin.Y)));//分割面上方的表面积
                            double pBelow = belowSA * invTotalSA;//射线穿过下包围盒的概率
                            double pAbove = aboveSA * invTotalSA;//射线穿过上包围盒的概率
                            double eb = (nAbove == 0 || nBelow == 0) ? emptyBonus : 0.0;//分割面为包围盒的一个面时，修正系数
                            double cost = traversalCost + isectCost * (1.0 - eb) * (pBelow * nBelow + pAbove * nAbove);//计算开销
                            // Update best split if this is lowest cost so far
                            if (cost < bestCost)
                            {
                                bestCost = cost;
                                bestAxis = axis;
                                bestOffset = i;
                            }
                        } break;
                    default:
                        throw new Exception { };
                }
                if (edges[axis][i].type == BoundEdge.EdgeType.Start) ++nBelow;
            }
            if (!(nBelow == tris.Count && nAbove == 0))
                throw new Exception { };
            // Create leaf if no good splits were found 如果分割面不理想，创建叶节点
            if (bestAxis == -1 && retries < 2)
            {
                ++retries;
                axis = (axis + 1) % 3;
                goto retrySplit;
            }
            if (bestCost > oldCost) ++badRefines;
            if ((bestCost > 4.0 * oldCost && tris.Count < 16) || bestAxis == -1 || badRefines == 3)
            { nodes[nodeNum].InitLeaf(tris); return; }

            // Classify primitives with respect to split 根据分割面，将分割面两侧的三角面编号存入两个数组
            int n0 = 0, n1 = 0;
            for (int i = 0; i < bestOffset; ++i)
                if (edges[bestAxis][i].type == BoundEdge.EdgeType.Start)
                    prims0[n0++] = edges[bestAxis][i].primNum;
            for (int i = bestOffset + 1; i < 2 * tris.Count; ++i)
                if (edges[bestAxis][i].type == BoundEdge.EdgeType.End)
                    prims0[n1++] = edges[bestAxis][i].primNum;

            // Recursively initialize children nodes 递归创建子节点
            double tsplit = edges[bestAxis][bestOffset].t;
            Bounds3 bounds0 = nodeBounds, bound1 = nodeBounds;
            switch (axis)
            {
                case 0: bounds0.pMax.X = bound1.pMin.X = tsplit; break;
                case 1: bounds0.pMax.Y = bound1.pMin.Y = tsplit; break;
                case 2: bounds0.pMax.Z = bound1.pMin.Z = tsplit; break;
                default: throw new Exception { };
            }
            //待补充 本文件数据结构与参考书不同
        }

        private bool Intersect(RayInfo ray)
        {
            // Compute initial parametric range of ray inside kd-tree extent
            double tmin = 0.000001;
            double tmax = Double.PositiveInfinity;
            if (!bounds.IntersectP(ray, ref tmin, ref tmax))
                return false;

            // Prepare to traverse kd-tree for ray
            SpectVector invDir = new SpectVector(1.0 / ray.RayVector.a, 1.0 / ray.RayVector.b, 1.0 / ray.RayVector.c);
            List<KdToDo> todo = new List<KdToDo>();
            int todoPos = 0;

            // Traverse kd-tree nodes in order for ray
            bool hit = false;
            KDNode node = nodes[0];
            while (node != null)
            {
                // Bail out if we found a hit closer than the current node
                if (ray.maxt < tmin) break;
                if (!node.IsLeaf())
                {
                    // Process kd-tree interior node

                    // Compute parametric distance along ray to split plane
                    int axis = node.SplitAxis();
                    switch (axis)
                    {
                        case 0:
                            {
                                double tplane = (node.splitPlane - ray.Origin.X) * invDir.a;
                                // Get node children pointers for ray 根据光线穿越包围盒的先后情况决定处理子节点的顺序
                                KDNode firstChild = new KDNode();
                                KDNode secondChild = new KDNode();
                                bool isBelowFirst = (ray.Origin.X < node.splitPlane) ||
                                    (ray.Origin.X == node.splitPlane && ray.RayVector.a <= 0);
                                if (isBelowFirst)
                                {
                                    //firstChild = node + 1;
                                    //secondChild = &nodes[node->AboveChild()];
                                }
                                else
                                {
                                    //firstChild = &nodes[node->AboveChild()];
                                    //secondChild = node + 1;
                                }
                                // Advance to next child node, possibly enqueue other child 有一些无需处理本节点的全部两个子节点的情况
                                if (tplane > tmax || tplane <= 0)
                                    node = firstChild;
                                else if (tplane < tmin)
                                    node = secondChild;
                                else
                                {
                                    // Enqueue _secondChild_ in todo list
                                    todo[todoPos].node = secondChild;
                                    todo[todoPos].tmin = tplane;
                                    todo[todoPos].tmax = tmax;
                                    ++todoPos;
                                    node = firstChild;
                                    tmax = tplane;
                                }
                            } break;
                        case 1:
                            {
                                double tplane = (node.splitPlane - ray.Origin.Y) * invDir.b;
                                // Get node children pointers for ray 根据光线穿越包围盒的先后情况决定处理子节点的顺序
                                KDNode firstChild = new KDNode();
                                KDNode secondChild = new KDNode();
                                bool isBelowFirst = (ray.Origin.Y < node.splitPlane) ||
                                    (ray.Origin.Y == node.splitPlane && ray.RayVector.b <= 0);
                                if (isBelowFirst)
                                {
                                    //firstChild = node + 1;
                                    //secondChild = &nodes[node->AboveChild()];
                                }
                                else
                                {
                                    //firstChild = &nodes[node->AboveChild()];
                                    //secondChild = node + 1;
                                }
                                // Advance to next child node, possibly enqueue other child 有一些无需处理本节点的全部两个子节点的情况
                                if (tplane > tmax || tplane <= 0)
                                    node = firstChild;
                                else if (tplane < tmin)
                                    node = secondChild;
                                else
                                {
                                    // Enqueue _secondChild_ in todo list
                                    todo[todoPos].node = secondChild;
                                    todo[todoPos].tmin = tplane;
                                    todo[todoPos].tmax = tmax;
                                    ++todoPos;
                                    node = firstChild;
                                    tmax = tplane;
                                }
                            } break;
                        case 2:
                            {
                                double tplane = (node.splitPlane - ray.Origin.Z) * invDir.c;
                                // Get node children pointers for ray 根据光线穿越包围盒的先后情况决定处理子节点的顺序
                                KDNode firstChild = new KDNode();
                                KDNode secondChild = new KDNode();
                                bool isBelowFirst = (ray.Origin.Z < node.splitPlane) ||
                                    (ray.Origin.Z == node.splitPlane && ray.RayVector.c <= 0);
                                if (isBelowFirst)
                                {
                                    //firstChild = node + 1;
                                    //secondChild = &nodes[node->AboveChild()];
                                }
                                else
                                {
                                    //firstChild = &nodes[node->AboveChild()];
                                    //secondChild = node + 1;
                                }
                                // Advance to next child node, possibly enqueue other child 有一些无需处理本节点的全部两个子节点的情况
                                if (tplane > tmax || tplane <= 0)
                                    node = firstChild;
                                else if (tplane < tmin)
                                    node = secondChild;
                                else
                                {
                                    // Enqueue _secondChild_ in todo list
                                    todo[todoPos].node = secondChild;
                                    todo[todoPos].tmin = tplane;
                                    todo[todoPos].tmax = tmax;
                                    ++todoPos;
                                    node = firstChild;
                                    tmax = tplane;
                                }
                            } break;
                        default: throw new Exception { };
                    }
                }
                else
                {
                    // Check for intersections inside leaf node
                    int nPrimitives = node.primitives.Count;
                    if (nPrimitives == 1)
                    { 
                        
                    }
                }
            }
        }


    }

    public class Bounds3
    {
        public Point pMin=new Point();
        public Point pMax=new Point();

        public Bounds3()
        {
            this.pMin.X = Double.NegativeInfinity;
            this.pMin.Y = Double.NegativeInfinity;
            this.pMin.Z = Double.NegativeInfinity;
            this.pMax.X = Double.PositiveInfinity;
            this.pMax.Y = Double.PositiveInfinity;
            this.pMax.Z = Double.PositiveInfinity;
        }
        /// <summary>
        /// 给出空间的两个点，创造包围盒
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Bounds3(Point p1, Point p2)
        {
            pMin.X = Math.Min(p1.X, p2.X);
            pMin.Y = Math.Min(p1.Y, p2.Y);
            pMin.Z = Math.Min(p1.Z, p2.Z);
            pMax.X = Math.Max(p1.X, p2.X);
            pMax.Y = Math.Max(p1.Y, p2.Y);
            pMax.Z = Math.Max(p1.Z, p2.Z);
        }

        /// <summary>
        /// 本包围盒和空间一点组成新包围盒
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Bounds3 Union(Point p)
        {
            Bounds3 b = new Bounds3();
            b.pMin.X = Math.Min(this.pMin.X, p.X);
            b.pMin.Y = Math.Min(this.pMin.Y, p.Y);
            b.pMin.Z = Math.Min(this.pMin.Z, p.Z);
            b.pMax.X = Math.Max(this.pMax.X, p.X);
            b.pMax.Y = Math.Max(this.pMax.Y, p.Y);
            b.pMax.Z = Math.Max(this.pMax.Z, p.Z);
            return b;

        }

        /// <summary>
        /// 两个包围盒组成一个新包围盒
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public Bounds3 Union(Bounds3 b)
        {
            Bounds3 b1 = new Bounds3();
            b1.pMin.X = Math.Min(this.pMin.X, b.pMin.X);
            b1.pMin.Y = Math.Min(this.pMin.Y, b.pMin.Y);
            b1.pMin.Z = Math.Min(this.pMin.Z, b.pMin.Z);
            b1.pMax.X = Math.Max(this.pMax.X, b.pMax.X);
            b1.pMax.Y = Math.Max(this.pMax.Y, b.pMax.Y);
            b1.pMax.Z = Math.Max(this.pMax.Z, b.pMax.Z);
            return b1;
        }

        /// <summary>
        /// 包围盒表面积
        /// </summary>
        /// <returns></returns>
        public double SurfaceArea()
        {
            return 2 * ((this.pMax.X - this.pMin.X) * (this.pMax.Y - this.pMin.Y) +
                (this.pMax.X - this.pMin.X) * (this.pMax.Z - this.pMin.Z) + 
                (this.pMax.Y - this.pMin.Y) * (this.pMax.Z - this.pMin.Z));
        }

        /// <summary>
        /// 包围盒的最长轴
        /// </summary>
        /// <returns></returns>
        public int MaximumExtent()
        {
            double x = this.pMax.X - this.pMin.X;
            double y = this.pMax.Y - this.pMin.Y;
            double z = this.pMax.Z - this.pMin.Z;
            if (x > y && x > z)
                return 0;
            else if (y > z)
                return 1;
            else
                return 2;
        }

        public bool IntersectP(RayInfo ray, ref double hitt0, ref double hitt1)
        {
            double t0 = 0.000001;
            double t1 = Double.PositiveInfinity;
            for (int i = 0; i < 3; i++)
            {
                // Update interval for _i_th bounding box slab 更新三组包围盒平行板间距
                switch (i)
                {
                    case 0:
                        {
                            double invDir = 1.0 / ray.RayVector.a;
                            double tNear = (pMin.X - ray.Origin.X) * invDir;
                            double tFar = (pMax.X - ray.Origin.X) * invDir;
                            // Update parametric interval from slab intersection $t$s
                            if (tNear > tFar)
                            {
                                double temp = tNear;
                                tNear = tFar;
                                tFar = temp;
                            }
                            t0 = tNear > t0 ? tNear : t0;
                            t1 = tFar < t1 ? tFar : t1;
                            if (t0 > t1) return false;
                        }break;
                    case 1:
                        {
                            double invDir = 1.0 / ray.RayVector.b;
                            double tNear = (pMin.Y - ray.Origin.Y) * invDir;
                            double tFar = (pMax.Y - ray.Origin.Y) * invDir;
                            // Update parametric interval from slab intersection $t$s
                            if (tNear > tFar)
                            {
                                double temp = tNear;
                                tNear = tFar;
                                tFar = temp;
                            }
                            t0 = tNear > t0 ? tNear : t0;
                            t1 = tFar < t1 ? tFar : t1;
                            if (t0 > t1) return false;
                        }break;
                    case 2:
                        {
                            double invDir = 1.0 / ray.RayVector.c;
                            double tNear = (pMin.Z - ray.Origin.Z) * invDir;
                            double tFar = (pMax.Z - ray.Origin.Z) * invDir;
                            // Update parametric interval from slab intersection $t$s
                            if (tNear > tFar)
                            {
                                double temp = tNear;
                                tNear = tFar;
                                tFar = temp;
                            }
                            t0 = tNear > t0 ? tNear : t0;
                            t1 = tFar < t1 ? tFar : t1;
                            if (t0 > t1) return false;
                        }break;
                    default:
                        throw new Exception { };                     
                }                
            }
            if (hitt0 != Double.NaN) hitt0 = t0;
            if (hitt1 != Double.NaN) hitt1 = t1;
            return true;
        }
    }

    public class KdToDo
    {
        public KDNode node = new KDNode();
        public double tmin, tmax;
    }

    
}
