using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculateModelClasses
{
    public class KDNode : ICloneable
    {
        public KDNode left;//左子节点
        public KDNode right;//右子节点
        public double splitPlane;//分割面坐标
        public int flag;//区分基于x,y,z轴划分的内部节点（对应0,1,2）以及叶节点（对应3）
        public List<int> primitiveNumbers = new List<int>();//包含的三角面的对应编号

        public KDNode()
        {
            this.splitPlane = Double.NaN;
            this.flag = -1;
        }
        /// <summary>
        /// 建立叶节点
        /// </summary>
        /// <param name="primNums">该节点包含的三角面编号</param>
        /// <param name="np">该节点包含的三角面个数</param>
        public void InitLeaf(int[] primNums, int np)
        {
            for (int i = 0; i < np; i++)
            {
                primitiveNumbers.Add(primNums[i]);//深拷贝
            }
            this.flag = 3;//叶节点对应的标志位编号
        }
        /// <summary>
        /// 建立内部节点
        /// </summary>
        /// <param name="axis">分割轴（0，1，2对应X，Y，Z轴）</param>
        /// <param name="splitPlane">分割面坐标</param>
        public void InitInterior(int axis, double splitPlane)
        {
            this.splitPlane = splitPlane;
            this.flag = axis;
        }

        public bool IsLeaf()
        {
            return flag == 3 ? true : false;
        }

        /// <summary>
        /// 拷贝节点对象，对象中的KDNode类型的子节点使用浅拷贝即可（深拷贝会陷入循环）
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            KDNode newKDNode = new KDNode();          
            newKDNode.left = this.left;
            newKDNode.right = this.right;
            newKDNode.splitPlane = this.splitPlane;
            newKDNode.flag = this.flag;
            newKDNode.primitiveNumbers = this.primitiveNumbers;
            return newKDNode;
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
        public KDNode rootNode = new KDNode();//记录根节点
        private Bounds3 bounds;//整个场景的大包围盒
        private int maxDepth;
        /// <summary>
        /// 构造KDTree加速器
        /// </summary>
        /// <param name="primitives">三角面</param>
        /// <param name="isectCost">判交成本</param>
        /// <param name="traversalCost">遍历成本</param>
        /// <param name="emptyBonus">两区域其一为空时，附加值参数，介于0到1之间</param>
        /// <param name="maxPrims">单个节点允许的最大三角面数量</param>
        /// <param name="maxDepth">允许的最大树深度</param>
        public KDTreeAccelerator(List<Triangle> primitives, int isectCost = 80, int traversalCost = 1,
            double emptyBonus = 0.0, int maxPrims = 1, int maxDepth = -1)
        {
            this.isectCost = isectCost;
            this.traversalCost = traversalCost;
            this.maxPrims = maxPrims;
            this.emptyBonus = emptyBonus;
            this.primitives = primitives;
            this.maxDepth = maxDepth;

            //Build kd-Tree for accelerator
            if (this.maxDepth <= 0)
                this.maxDepth = (int)Math.Round(8 + 1.3 * (Math.Log(primitives.Count) / Math.Log(2)), 0);

            //Compute bounds for kd-tree construction 计算根节点的包围盒
            List<Bounds3> primBounds = new List<Bounds3>();
            bounds = primitives[0].WorldBound();
            for (int i = 0; i < primitives.Count; i++)
            {
                Bounds3 tempBound = primitives[i].WorldBound();
                bounds = bounds.Union(tempBound);
                primBounds.Add(tempBound);
            }

            // Initialize _primNums_ for kd-tree construction 初始化面元编号数组
            int[] primNums = new int[primitives.Count];
            for (int i = 0; i < primitives.Count; i++)
            {
                primNums[i] = i;
            }

            // Start recursive construction of kd-tree 开始递归建树
            BuildTree(rootNode, bounds, primBounds, primNums, primitives.Count, this.maxDepth, 0);
        }
        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="currentNode">当前节点</param>
        /// <param name="nodeBounds">当前节点的包围盒</param>
        /// <param name="allPrimBounds">所有面元的包围盒</param>
        /// <param name="primNums">当前节点所包含的面元编号数组</param>
        /// <param name="nPrimitives">当前节点所包含的面元数量</param>
        /// <param name="depth">当前节点深度</param>
        /// <param name="badRefines">到当前节点为止的非优划分次数</param>
        private void BuildTree(KDNode currentNode, Bounds3 nodeBounds, List<Bounds3> allPrimBounds,
            int[] primNums, int nPrimitives, int depth, int badRefines)
        {
            Console.WriteLine("正在构建第{0}层树", this.maxDepth - depth);
            // Initialize leaf node if termination criteria met
            if (nPrimitives <= this.maxPrims || depth == 0)
            {
                currentNode.InitLeaf(primNums, nPrimitives);
                return;
            }
            // Allocate working memory for kd-tree construction C#中这一步已简化
            List<List<BoundEdge>> edges = new List<List<BoundEdge>>();
            for (int i = 0; i < 3; i++)
            {
                List<BoundEdge> edge = new List<BoundEdge>();
                for (int j = 0; j < 2 * nPrimitives; j++)
                {
                    edge.Add(new BoundEdge());
                }
                edges.Add(edge);
            }
            int[] prims0 = new int[primitives.Count];
            int[] prims1 = new int[primitives.Count];

            // Initialize interior node and continue recursion 初始化内部节点并递归创建

            // Choose split axis position for interior node 为内部节点选择分割轴位置
            int bestAxis = -1, bestOffset = -1;
            double bestCost = Double.PositiveInfinity;
            double oldCost = isectCost * (double)(nPrimitives);//与所有面判交成本
            double totalSA = nodeBounds.SurfaceArea();//当前包围盒表面积
            double invTotalSA = 1.0 / totalSA;
            SpectVector d = new SpectVector(nodeBounds.pMin, nodeBounds.pMax);
            // Choose which axis to split along 选择最长的坐标轴作为分割轴
            int axis = nodeBounds.MaximumExtent();
            int retries = 0;
        retrySplit:
            // Initialize edges for _axis_
            List<BoundEdge> bes = new List<BoundEdge>();
            for (int i = 0; i < nPrimitives; i++)
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
            Sort(bes);//对包围盒在轴线的投影进行排序
            for (int i = 0; i < nPrimitives; i++)
            {
                edges[axis][2 * i] = bes[2 * i];
                edges[axis][2 * i + 1] = bes[2 * i + 1];
            }
            // Compute cost of all splits for _axis_ to find best
            int nBelow = 0, nAbove = nPrimitives;
            for (int i = 0; i < 2 * nPrimitives; i++)
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
                            double cost = traversalCost + isectCost * (pBelow * nBelow + pAbove * nAbove);//计算开销
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
                            double cost = traversalCost + isectCost * (pBelow * nBelow + pAbove * nAbove);//计算开销
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
                            double cost = traversalCost + isectCost * (pBelow * nBelow + pAbove * nAbove);//计算开销
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

            if (!(nBelow == nPrimitives && nAbove == 0))
                throw new Exception { };
            // Create leaf if no good splits were found 如果分割面不理想，创建叶节点
            if (bestAxis == -1 && retries < 2)//当前轴划分不理想，则换一条轴重新划分
            {
                ++retries;
                axis = (axis + 1) % 3;
                goto retrySplit;
            }
            if (bestCost > oldCost) ++badRefines;
            if ((bestCost > 4.0 * oldCost && nPrimitives < 16) || bestAxis == -1 || badRefines == 3)
            { currentNode.InitLeaf(primNums, nPrimitives); return; }

            // Classify primitives with respect to split 根据分割面，将分割面两侧的三角面编号存入两个数组
            int n0 = 0, n1 = 0;
            for (int i = 0; i < bestOffset; ++i)
                if (edges[bestAxis][i].type == BoundEdge.EdgeType.Start)
                    prims0[n0++] = edges[bestAxis][i].primNum;
            for (int i = bestOffset + 1; i < 2 * nPrimitives; ++i)
                if (edges[bestAxis][i].type == BoundEdge.EdgeType.End)
                    prims1[n1++] = edges[bestAxis][i].primNum;

            // Recursively initialize children nodes 递归创建子节点
            double tsplit = edges[bestAxis][bestOffset].t;
            Bounds3 bounds0 = new Bounds3(nodeBounds.pMin, nodeBounds.pMax);
            Bounds3 bounds1 = new Bounds3(nodeBounds.pMin, nodeBounds.pMax);//一定要深拷贝，直接bound0=nodeBounds会因为浅拷贝而出错
            switch (axis)
            {
                case 0: bounds0.pMax.X = tsplit; bounds1.pMin.X = tsplit; break;
                case 1: bounds0.pMax.Y = tsplit; bounds1.pMin.Y = tsplit; break;
                case 2: bounds0.pMax.Z = tsplit; bounds1.pMin.Z = tsplit; break;
                default: throw new Exception { };
            }
            //递归创建子节点
            KDNode leftChild = new KDNode();
            currentNode.left = leftChild;//浅拷贝
            KDNode rightChild = new KDNode();
            currentNode.right = rightChild;//浅拷贝
            BuildTree(leftChild, bounds0, allPrimBounds, prims0, n0, depth - 1, badRefines);
            currentNode.InitInterior(bestAxis, tsplit);//中序建树
            BuildTree(rightChild, bounds1, allPrimBounds, prims1, n1, depth - 1, badRefines);
        }
        /// <summary>
        /// KDTree中的判交遍历
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public Node Intersect(RayInfo ray)
        {
            // Compute initial parametric range of ray inside kd-tree extent 计算KDtree中t的初始参数范围
            double tmin = 0.000001;
            double tmax = Double.PositiveInfinity;
            if (!bounds.IntersectP(ray, ref tmin, ref tmax))
                return null;

            // Prepare to traverse kd-tree for ray
            SpectVector invDir = new SpectVector(ray.RayVector.Mag() / ray.RayVector.a, ray.RayVector.Mag() / ray.RayVector.b, ray.RayVector.Mag() / ray.RayVector.c);
            List<KdToDo> todo = new List<KdToDo>();//需要处理的节点list 给一个足够大的初始化值64
            for (int i = 0; i < 64; i++)
            {
                todo.Add(new KdToDo());
            }
            int todoPos = 0;

            // Traverse kd-tree nodes in order for ray
            Node finalCrossPoint = null;
            KDNode node = (KDNode)rootNode.Clone();//需要深拷贝
            while (node.flag != -1)
            {
                // Bail out if we found a hit closer than the current node 如果当前节点的tmin比已找到的交点远，停止判断
                if (ray.maxt < tmin) break;
                if (!node.IsLeaf())
                {
                    // Process kd-tree interior node

                    // Compute parametric distance along ray to split plane
                    int axis = node.flag;
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
                                    firstChild = (KDNode)node.left.Clone();
                                    secondChild = (KDNode)node.right.Clone();
                                }
                                else
                                {
                                    //firstChild = &nodes[node->AboveChild()];
                                    //secondChild = node + 1;
                                    firstChild = (KDNode)node.right.Clone();
                                    secondChild = (KDNode)node.left.Clone();
                                }
                                // Advance to next child node, possibly enqueue other child 有一些无需处理本节点的全部两个子节点的情况
                                if (tplane > tmax || tplane <= 0)
                                    node = (KDNode)firstChild.Clone();
                                else if (tplane < tmin)
                                    node = (KDNode)secondChild.Clone();
                                else
                                {
                                    // Enqueue _secondChild_ in todo list
                                    todo[todoPos].node = (KDNode)secondChild.Clone();
                                    todo[todoPos].tmin = tplane;
                                    todo[todoPos].tmax = tmax;
                                    ++todoPos;
                                    node = (KDNode)firstChild.Clone();
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
                                    firstChild = (KDNode)node.left.Clone();
                                    secondChild = (KDNode)node.right.Clone();
                                }
                                else
                                {
                                    //firstChild = &nodes[node->AboveChild()];
                                    //secondChild = node + 1;
                                    firstChild = (KDNode)node.right.Clone();
                                    secondChild = (KDNode)node.left.Clone();
                                }
                                // Advance to next child node, possibly enqueue other child 有一些无需处理本节点的全部两个子节点的情况
                                if (tplane > tmax || tplane <= 0)
                                    node = (KDNode)firstChild.Clone();
                                else if (tplane < tmin)
                                    node = (KDNode)secondChild.Clone();
                                else
                                {
                                    // Enqueue _secondChild_ in todo list
                                    todo[todoPos].node = (KDNode)secondChild.Clone();
                                    todo[todoPos].tmin = tplane;
                                    todo[todoPos].tmax = tmax;
                                    ++todoPos;
                                    node = (KDNode)firstChild.Clone();
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
                                    firstChild = (KDNode)node.left.Clone();
                                    secondChild = (KDNode)node.right.Clone();
                                }
                                else
                                {
                                    //firstChild = &nodes[node->AboveChild()];
                                    //secondChild = node + 1;
                                    firstChild = (KDNode)node.right.Clone();
                                    secondChild = (KDNode)node.left.Clone();
                                }
                                // Advance to next child node, possibly enqueue other child 有一些无需处理本节点的全部两个子节点的情况
                                if (tplane > tmax || tplane <= 0)
                                    node = (KDNode)firstChild.Clone();
                                else if (tplane < tmin)
                                    node = (KDNode)secondChild.Clone();
                                else
                                {
                                    // Enqueue _secondChild_ in todo list
                                    todo[todoPos].node = (KDNode)secondChild.Clone();
                                    todo[todoPos].tmin = tplane;
                                    todo[todoPos].tmax = tmax;
                                    ++todoPos;
                                    node = (KDNode)firstChild.Clone();
                                    tmax = tplane;
                                }
                            } break;
                        default: throw new Exception { };
                    }
                }
                else
                {
                    // Check for intersections inside leaf node
                    int nPrimitives = node.primitiveNumbers.Count;
                    List<int> primNums = node.primitiveNumbers;
                    for (int i = 0; i < nPrimitives; i++)
                    {
                        Triangle tri = primitives[primNums[i]];
                        Node crossPoint = ray.GetCrossNodeWithOriginTriangle(tri);
                        if (crossPoint != null && crossPoint.DistanceToFrontNode < ray.maxt)
                        {
                            ray.maxt = crossPoint.DistanceToFrontNode;
                            finalCrossPoint = crossPoint;
                        }

                    }
                    // Grab next node to process from todo list
                    if (todoPos > 0)
                    {
                        --todoPos;
                        node = (KDNode)todo[todoPos].node.Clone();
                        tmin = todo[todoPos].tmin;
                        tmax = todo[todoPos].tmax;
                    }
                    else
                        break;
                }
            }
            return finalCrossPoint;
        }

        /// <summary>
        /// 快速排序BoundEdge结构体
        /// </summary>
        /// <param name="be">待排序的结构体</param>
        /// <param name="low">起始下标</param>
        /// <param name="high">结束下标</param>
        private static void Sort(List<BoundEdge> be)
        {
            //先使用冒泡排序，确保能跑通，之后再改快排
            for (int i = 0; i < be.Count; i++)
            {
                for (int j = 0; j < be.Count - 1 - i; j++)
                {
                    if (be[j] > be[j + 1])
                    {
                        BoundEdge temp = be[j];
                        be[j] = be[j + 1];
                        be[j + 1] = temp;
                    }
                }
            }
        }
    }

    public class Bounds3
    {
        public Point pMin = new Point();
        public Point pMax = new Point();

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

        /// <summary>
        /// 计算射线与包围盒（三组平行板）的交点范围
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="hitt0"></param>
        /// <param name="hitt1"></param>
        /// <returns></returns>
        public bool IntersectP(RayInfo ray, ref double hitt0, ref double hitt1)
        {
            double t0 = ray.mint;
            double t1 = ray.maxt;
            for (int i = 0; i < 3; i++)
            {
                // Update interval for _i_th bounding box slab 更新三组包围盒平行板间距
                switch (i)
                {
                    case 0:
                        {
                            double invDir = ray.RayVector.Mag() / ray.RayVector.a;
                            double tNear = (this.pMin.X - ray.Origin.X) * invDir;
                            double tFar = (this.pMax.X - ray.Origin.X) * invDir;
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
                        } break;
                    case 1:
                        {
                            double invDir = ray.RayVector.Mag() / ray.RayVector.b;
                            double tNear = (this.pMin.Y - ray.Origin.Y) * invDir;
                            double tFar = (this.pMax.Y - ray.Origin.Y) * invDir;
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
                        } break;
                    case 2:
                        {
                            double invDir = ray.RayVector.Mag() / ray.RayVector.c;
                            double tNear = (this.pMin.Z - ray.Origin.Z) * invDir;
                            double tFar = (this.pMax.Z - ray.Origin.Z) * invDir;
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
                        } break;
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
