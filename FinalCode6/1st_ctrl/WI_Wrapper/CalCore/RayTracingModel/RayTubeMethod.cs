using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CalculateModelClasses;
using LogFileManager;
using CityAndFloor;
using UanFileProceed;

namespace RayCalInfo
{
    public class RayTubeMethod
    {
        private Node tx;
        private List<Path> receivedPaths;

        public List<Path> ReceivedPaths
        {
            get { return this.receivedPaths; }
        }

        public RayTubeMethod(Node tx, ReceiveBall rxBall, Terrain ter, City buildings, int firstN)
        {
            this.tx = tx;
            this.receivedPaths = this.GetPunctiformRxPaths(rxBall, ter, buildings, firstN);
        }
        //态势接收
        public RayTubeMethod(Node tx, ReceiveArea reArea, Terrain ter, City buildings, int firstN)
        {
            this.tx = tx;
            this.GetAreaSituationRxPaths(tx, reArea, ter, buildings, firstN);
        }

        /// <summary>
        ///获取所有射线的路径
        /// </summary>
        /// <returns></returns>
        public void ReverseTracingPathsAndDeleteRepeatedPaths()
        {
            //反向追踪射线，把错误的射线删除
            for (int i = this.receivedPaths.Count - 1; i >= 0; i--)
            {
                this.receivedPaths[i] = this.receivedPaths[i].UpdateReversePaths();
                if (this.receivedPaths[i] == null)
                {
                    this.receivedPaths.RemoveAt(i);
                }
            }
            //删除重复的射线
            this.DeleteRepeatedPath(this.receivedPaths);
        }

        public void ReverseAreaTracingPathsAndDeleteRepeatedPaths(List<Path> areaNodePaths)
        {
            //反向追踪射线，把错误的射线删除
            for (int i =areaNodePaths.Count - 1; i >= 0; i--)
            {
                areaNodePaths[i] = areaNodePaths[i].UpdateReversePaths();
                if (areaNodePaths[i] == null)
                {
                    areaNodePaths.RemoveAt(i);
                }
                else
                {
                    for(int a=1;a<areaNodePaths[i].node.Count;a++)
                    {
                        areaNodePaths[i].node[a].RayIn = new RayInfo(areaNodePaths[i].node[a-1], areaNodePaths[i].node[a]);
                    }
                }
            }
            //删除重复的射线
            this.DeleteRepeatedPath(areaNodePaths);
        }
        /// <summary>
        /// 根据不同的频率进行场强，功率的计算
        /// </summary>
        ///  <param name="TxFrequencyBand">频段信息</param>
        /// <returns>筛选后的路径</returns>
        public List<List<Path>> ScreenPunctiformPathsByFrequencyAndCalculateEField(List<FrequencyBand> TxFrequencyBand)
        {
            List<List<Path>> classifiedPath = new List<List<CalculateModelClasses.Path>>();
            if (this.receivedPaths.Count == 0)//若没有射线，加入空的链表到输出结果中
            {
                for (int c = 0; c < TxFrequencyBand.Count; c++)
                { classifiedPath.Add(new List<Path>()); }
            }
            else
            {
                for (int i = 0; i < TxFrequencyBand.Count; i++)//对频段进行遍历
                {
                    List<Path> tempPath = new List<CalculateModelClasses.Path>();
                    for (int j = 0; j < this.receivedPaths.Count; j++)
                    {
                        List<Node> pathNodes = new List<Node>();
                        for (int n = 0; n < this.receivedPaths[j].node.Count; n++)//对该射线除了第一个Tx点外的节点进行计算
                        {
                            Node temp = (Node)this.receivedPaths[j].node[n].Clone();

                            if (n == 0)
                            {
                                temp.Power = TxFrequencyBand[i].Power;
                            }
                            temp.Frequence = TxFrequencyBand[i].MidPointFrequence;
                            temp.FrequenceWidth = TxFrequencyBand[i].FrequenceWidth;
                            if (n != 0)
                            {
                                this.GetEfield(temp.UAN, pathNodes[n - 1], ref temp);//计算场强
                                this.GetEfield(this.receivedPaths[j].node[0].UAN, pathNodes[n - 1], ref temp);
                                //接收点求功率,有问题，电导率和节点常数是面的还是空气的
                                temp.Power = Power.GetPower(temp, pathNodes[n - 1])[0];//计算功率
                            }
                            pathNodes.Add(temp);
                        }
                        Path midpath = new CalculateModelClasses.Path(pathNodes);

                        if (midpath.Rxnum != 0)
                        {
                            this.GetLossAndComponentOFPath(midpath);
                            tempPath.Add(midpath);
                        }

                    }
                    classifiedPath.Add(tempPath);

                }
            }
            return classifiedPath;
        }

        /// <summary>
        /// 根据不同的频率进行场强，功率的计算
        /// </summary>
        ///  <param name="TxFrequencyBand">频段信息</param>
        /// <returns>筛选后的路径</returns>
        public void ScreenAreaSituationPathsByFrequencyAndCalculateEField(List<FrequencyBand> TxFrequencyBand, List<Path> receivedPaths, List<List<Path>> classifiedPath)
        {
            if (receivedPaths.Count == 0)//若没有射线，加入空的链表到输出结果中
            {
                for (int c = 0; c < TxFrequencyBand.Count; c++)
                { classifiedPath.Add(new List<Path>()); }
            }
            else
            {
                for (int i = 0; i < TxFrequencyBand.Count; i++)//对频段进行遍历
                {
                    List<Path> tempPath = new List<CalculateModelClasses.Path>();
                    for (int j = 0; j < receivedPaths.Count; j++)
                    {
                        List<Node> pathNodes = new List<Node>();
                        for (int n = 0; n < receivedPaths[j].node.Count; n++)//对该射线除了第一个Tx点外的节点进行计算
                        {
                            Node temp = (Node)receivedPaths[j].node[n].Clone();

                            if (n == 0)
                            {
                                temp.Power = TxFrequencyBand[i].Power;
                            }
                            temp.Frequence = TxFrequencyBand[i].MidPointFrequence;
                            temp.FrequenceWidth = TxFrequencyBand[i].FrequenceWidth;
                            if (n != 0)
                            {
                                this.GetEfield(temp.UAN, pathNodes[n - 1], ref temp);//计算场强
                                //接收点求功率,有问题，电导率和节点常数是面的还是空气的
                               //temp.Power = Power.GetPower(temp, pathNodes[n - 1])[0];//计算功率
                            }
                            pathNodes.Add(temp);
                        }
                        Path midpath = new CalculateModelClasses.Path(pathNodes);

                        if (midpath.Rxnum != 0)
                        {
                            this.GetLossAndComponentOFPath(midpath);
                            tempPath.Add(midpath);
                        }
                    }
                    classifiedPath.Add(tempPath);
                }
            }
        }


        /// <summary>
        ///获取所有射线的路径
        /// </summary>
        /// <param name="ter">地形</param>
        /// <param name="rxBall">接收球</param>
        ///  <param name="buildings">建筑物</param>
        ///  <param name="firstN">初始细分点个数</param>
        /// <returns></returns>
        private List<Path> GetPunctiformRxPaths(ReceiveBall rxBall, Terrain ter, City buildings, int firstN)
        {
            List<Path> receivedPaths = new List<Path>();
            List<RayTubeModel> originalUnits = this.GetOriginUnitsOfIcosahedron();//正二十面体的二十个三角面
            for (int i = 0; i < originalUnits.Count; i++)
            {
                Console.WriteLine(DateTime.Now);
                Stack<RayTubeModel> initialModels = this.GetInitialRayTubeModels(this.tx, originalUnits[i], firstN);
                receivedPaths.AddRange(this.GetPathsFromRayTubeModels(initialModels, ter, rxBall, buildings));
                Console.WriteLine(DateTime.Now);
            }

            return receivedPaths;
        }

        /// <summary>
        /// 获取所有射线路径
        /// </summary>
        /// <param name="tx">发射机</param>
        /// <param name="reArea">态势区域</param>
        /// <param name="ter">地形</param>
        /// <param name="building">建筑物</param>
        /// <param name="firstN">初始细分点个数</param>
        /// <returns></returns>
        private void GetAreaSituationRxPaths(Node tx, ReceiveArea reArea, Terrain ter, City buildings, int firstN)
        {
            List<RayTubeModel>[] tubesCrossWithAreaSituation = new List<RayTubeModel>[reArea.areaSituationRect.Count];
            for (int i = 0; i < reArea.areaSituationRect.Count; i++)
            {
                tubesCrossWithAreaSituation[i] = new List<RayTubeModel>();//初始化
            }
            List<RayTubeModel> originalUnits = this.GetOriginUnitsOfIcosahedron();//得到由正二十面体生成的初始的20个射线管模型

            //for (int i = 0; i < originalUnits.Count; i++)
                for (int i = 0; i < 20; i++)
                {
                Console.WriteLine("对正二十面体的第{0}个三角面进行射线追踪", i);
                Console.Write("起始时间："); Console.Write(DateTime.Now);
                Stack<RayTubeModel> initialModels = this.GetInitialRayTubeModels(this.tx, originalUnits[i], firstN);//初始化细分射线管
                this.GetRayTubesCrossWithAreaSituation(tubesCrossWithAreaSituation, initialModels, ter, reArea, buildings);//标记与各个态势层相交的射线管
                Console.Write(" 完成时间："); Console.Write(DateTime.Now); Console.WriteLine();
            }
            this.GetPathsFromTxToArea(tubesCrossWithAreaSituation, reArea);
        }
        /// <summary>
        /// 获得从发射点到态势区域的路径
        /// </summary>
        /// <param name="tubesCrossWithAreaSituation">到达各个态势层的射线管集合</param>
        /// <param name="reArea">态势区域</param>
        private void GetPathsFromTxToArea(List<RayTubeModel>[] tubesCrossWithAreaSituation, ReceiveArea reArea)
        {
            for (int i = 0; i < reArea.areaSituationRect.Count; i++)//按照态势层循环
            {
                Console.Write("正在处理第{0}个态势层,起始时间：", i); Console.Write(DateTime.Now);
                int rowDiscretedNum = 35;//行离散倍数，可更改 
                int lineDiscretedNum = 30;//列离散倍数，可更改
                double rowUnitLength = reArea.areaSituationRect[i].LeftBottomPoint.GetDistance(reArea.areaSituationRect[i].RightBottomPoint) / rowDiscretedNum;
                double lineUnitLength = reArea.areaSituationRect[i].LeftBottomPoint.GetDistance(reArea.areaSituationRect[i].LeftTopPoint) / lineDiscretedNum;              
                List<List<AreaNode>> areaSituationNodess = new List<List<AreaNode>>();
                areaSituationNodess=this.GetAreaNodesByDiscretedNums2D(reArea.areaSituationRect[i], rowDiscretedNum, lineDiscretedNum);
                foreach(RayTubeModel currentRayTube in tubesCrossWithAreaSituation[i])
                {
                    List<Point> points = new List<Point>();//存储外框的边界点
                    for (int m=0;m<currentRayTube.OneRayModels.Count;m++)
                    {
                        Node crossNode = currentRayTube.OneRayModels[m].LaunchRay.GetCrossNodeWithRect(reArea.areaSituationRect[i]);

                        if(crossNode!=null)
                        {
                            points.Add(new Point(crossNode.Position));
                        }
                    }
                    SpaceFace verticalFace = new SpaceFace(reArea.areaSituationRect[i].LeftBottomPoint, reArea.areaSituationRect[i].RightBottomPoint, reArea.areaSituationRect[i].LeftTopPoint);//作一个态势平面,态势层是平行于xoy的矩形
                    List<Point> crossPoints = new List<Point>();
                    for (int m = 0; m < currentRayTube.OneRayModels.Count; m++)//求射线管与该平面的交点
                    {
                        Point crossPoint1 = currentRayTube.OneRayModels[m].LaunchRay.GetCrossPointWithFace(verticalFace);
                        if (crossPoint1 != null)
                        { crossPoints.Add(crossPoint1); }
                    }
                    if(crossPoints.Count==currentRayTube.OneRayModels.Count)
                    {
                        //List<Point> crossPoints = new List<Point>(GetCrossPointsWithFace(reArea.areaSituationRect[i], currentRayTube));
                        //Point crossPoint = currentRayTube.OneRayModels[m].LaunchRay.GetCrossPointWithFace(reArea.areaSituationRect[i]);
                        Triangle rayTubeTriangle = new Triangle(crossPoints[0], crossPoints[1], crossPoints[2]);
                        List<Point> temp = rayTubeTriangle.GetCrossPointWithRect(reArea.areaSituationRect[i]);
                        for (int k = 0; k < temp.Count; k++)
                        {
                            if (temp[i] != null)
                            {
                                points.Add(temp[i]);
                            }
                        }
                        if (points != null && points.Count != 0)
                        {
                            points.OrderBy(Point => Point.X);//升序
                            Point xMinPoint = new Point(points[0]);
                            Point xMaxPoint = new Point(points[points.Count - 1]);
                            points.OrderBy(Point => Point.Y);//升序
                            Point yMinPoint = new Point(points[0]);
                            Point yMaxPoint = new Point(points[points.Count - 1]);
                            double xMin = xMinPoint.X;
                            double xMax = xMaxPoint.X;
                            double yMin = yMinPoint.Y;
                            double yMax = yMaxPoint.Y;
                            //找到四个点对应的m,n
                            int a1 = (int)Math.Floor((yMin - reArea.areaSituationRect[i].LeftBottomPoint.Y) / lineUnitLength);
                            a1 = a1 > 0 ? a1 : 0;
                            double aaa= (yMax - reArea.areaSituationRect[i].LeftBottomPoint.Y) / lineUnitLength;
                            int a2 = (int)Math.Ceiling((yMax - reArea.areaSituationRect[i].LeftBottomPoint.Y) / lineUnitLength);
                            a2 = a2 < lineDiscretedNum ? a2 : lineDiscretedNum;
                            int b1 = (int)Math.Floor((xMin - reArea.areaSituationRect[i].LeftBottomPoint.X) / rowUnitLength);
                            b1 = b1 > 0 ? b1 : 0;
                            int b2 = (int)Math.Ceiling((xMax - reArea.areaSituationRect[i].LeftBottomPoint.X) / rowUnitLength);
                            b2 = b2 < rowDiscretedNum ? a2 : rowDiscretedNum;
                            List<Node> pathNodes = new List<Node>();//存储从发射节点到当前射线管的发射节点所经过的路径节点
                            this.GetFrontNodesOfAreaSituationNodess(currentRayTube, pathNodes); //得到从发射点到当前射线管的list<node>
                                                                                                //射线管与态势层所在的平面所在的交点 3个
                                                                                                //List<Point> crossPoints =new List<Point>(GetCrossPointsWithFace(reArea.areaSituationRect[i], currentRayTube));
                            List<Point> crossPoints2 = GetCrossPointsWithFace(reArea.areaSituationRect[i], currentRayTube);
                            List<Point> crossPoints3 = new List<Point>();//存储当前射线管与态势层所在平面的交点
                            crossPoints3 = GetCrossPointsWithFace(reArea.areaSituationRect[i], currentRayTube);
                            for (int a = a1; a <= a2; a++)
                            {
                                for (int b = b1; b <= b2; b++)
                                {
                                    this.GetAreaNodeIfRayTubeContainPoint(currentRayTube, areaSituationNodess[a][b], pathNodes);//判断射线管是否到达虚拟接收点
                                }
                            }
                        }
                    }

                 
                    //points.AddRange(rayTubeTriangle.GetCrossPointWithRect(reArea.areaSituationRect[i]));

                }
                for (int j=0;j<areaSituationNodess.Count;j++)
                {
                    reArea.areaSituationNodes.AddRange(areaSituationNodess[j]);
                }
                //reArea.areaSituationNodes.AddRange(((List<AreaNode>areaSituationNodes.clone())));
                Console.Write("完成时间："); Console.Write(DateTime.Now); Console.WriteLine();
                //List<AreaNode> areaSituationNodes = new List<AreaNode>();
                ////将当前态势层离散为态势点
                //areaSituationNodes.AddRange(this.GetAreaNodesByDiscretedNum(reArea.areaSituationRect[i], rowDiscretedNum, lineDiscretedNum));
                ////遍历与当前态势层对应的射线管
                //foreach (RayTubeModel currentRayTube in tubesCrossWithAreaSituation[i])
                //{
                //    List<Node> pathNodes = new List<Node>();//存储从发射节点到当前射线管的发射节点所经过的路径节点
                //    this.GetFrontNodesOfAreaSituationNodess(currentRayTube, pathNodes); //得到从发射点到当前射线管的list<node>
                //    //可改进：遍历态势点，得到当前射线管包含的态势点并得出完整路径
                //    //正在改进for循环
                //    //挑选出射线管经过的点
                    

                //    for (int m = 0; m < areaSituationNodes.Count; m++)
                //    {
                //        //如果射线管经过此态势点，就会将从发射点到态势点的路径加至态势点的paths属性中
                  //this.GetAreaNodeIfRayTubeContainPoint(currentRayTube, areaSituationNodes[m], pathNodes);//判断射线管是否到达虚拟接收点
                //    }
                //}
                //reArea.areaSituationNodes.AddRange(areaSituationNodes);
                ////reArea.areaSituationNodes.AddRange(((List<AreaNode>areaSituationNodes.clone())));
                //Console.Write("完成时间："); Console.Write(DateTime.Now); Console.WriteLine();
            }
        }
        /// <summary>
        /// 获取射线管与平面的相交点
        /// </summary>
        /// <param name="rect">矩形</param>
        /// <param name="currentRayTube">射线管</param>
        /// <returns>交点链表</returns>
        private List<Point> GetCrossPointsWithFace(Rectangle rect,RayTubeModel currentRayTube)
        {
            List<Point> crossPoints = new List<Point>();
            SpaceFace verticalFace = new SpaceFace(rect.LeftBottomPoint, rect.LeftTopPoint, rect.RightBottomPoint);
            for (int i = 0; i < currentRayTube.OneRayModels.Count; i++)//求射线管与该平面的交点
            {
                Point crossPoint = currentRayTube.OneRayModels[i].LaunchRay.GetCrossPointWithFace(verticalFace);
                if (crossPoint != null)
                { crossPoints.Add(crossPoint); }
            }
            if(crossPoints.Count!=3)
            {
                LogFileManager.ObjLog.debug("获取射线管与态势层所在平面的相交点时出错");
            }
            return crossPoints;
        }


        /// <summary>
        /// 判断射线管是否包含虚拟接收点，将包含的点转化为节点，并将节点存储在list中
        /// </summary>
        /// <param name="currentRayTube">射线管</param>
        /// <param name="virtualRxPoint">接收点</param>
        /// <param name="crossNodeInArea">存储相交节点</param>
        /// <returns></returns>
        private void GetAreaNodeIfRayTubeContainPoint(RayTubeModel currentRayTube, AreaNode virtualRxNode, List<Node> pathNodes)
        {
            SpaceFace verticalFace = new SpaceFace(virtualRxNode.Position, new SpectVector(0, 0, 1));//作一个态势平面,态势层是平行于xoy的矩形
            List<Point> crossPoints = new List<Point>();
            for (int i = 0; i < currentRayTube.OneRayModels.Count; i++)//求射线管与该平面的交点
            {
                Point crossPoint = currentRayTube.OneRayModels[i].LaunchRay.GetCrossPointWithFace(verticalFace);
                if (crossPoint != null)
                { crossPoints.Add(crossPoint); }
            }
            if (crossPoints.Count != 3)
            {
                LogFileManager.ObjLog.error("判断射线管是否与态势层存在交点时出错");
            }
            else
            {
                Face wavefrontFace = new Triangle(crossPoints[0], crossPoints[1], crossPoints[2]);
                if (wavefrontFace.JudgeIfPointInFace(virtualRxNode.Position))////判断射线管是否包含当前态势点
                {
                    OneRayModel rayModelToRx = currentRayTube.StructureRayModelToTargetPoint(virtualRxNode.Position);//??
                    if (rayModelToRx == null)
                    {
                        LogFileManager.ObjLog.error("判断点是否在从绕射棱发出的射线管内时,构造到接收机的射线时出错");
                    }
                    else
                    {
                        //当前射线管包含态势点，可以生成路径啦~\(≧▽≦)/~
                        //构造接收节点
                        virtualRxNode.RxNum = 1;
                        virtualRxNode.NodeStyle = NodeStyle.Rx;
                        virtualRxNode.DistanceToFrontNode = rayModelToRx.LaunchNode.Position.GetDistance(virtualRxNode.Position);
                        virtualRxNode.RayIn = rayModelToRx.LaunchRay;
                        virtualRxNode.FatherNode = rayModelToRx.LaunchNode;
                        for (int i = 0; i < currentRayTube.OneRayModels.Count; i++)
                        {
                            currentRayTube.OneRayModels[i].LaunchNode.ChildNodes.Add(virtualRxNode);
                        }
                        virtualRxNode.FatherNode = currentRayTube.OneRayModels[0].LaunchNode;
                        pathNodes.Add(virtualRxNode);//得到从发射节点到态势点的完整的路径节点
                        Path onePath = new Path(new List<Node>(pathNodes));//生成一条新的路径
                        virtualRxNode.paths.Add(onePath);
                        pathNodes.RemoveAt(pathNodes.Count - 1);
                    }
                }
            }
        }
        /// <summary>
        /// 根据打到态势层上的射线管信息，获取从发射点到当前射线管的路径
        /// </summary>
        /// <param name="currentRayTube">当前射线管</param>
        /// <param name="nodes">从发射点到当前射线管的节点链表</param>
        private void GetFrontNodesOfAreaSituationNodes(RayTubeModel currentRayTube, List<Node> nodes)
        {
            //nodes.Add((Node)root.Clone());
            if (currentRayTube.FatherRayTube != null)
            {
                for (int i = 0; i < currentRayTube.OneRayModels.Count; i++)
                {
                    if (currentRayTube.OneRayModels[i].LaunchNode.FatherNode != null)
                    {
                        nodes.Insert(0, (Node)currentRayTube.OneRayModels[i].LaunchNode.Clone());
                        this.GetFrontNodesOfAreaSituationNodes(currentRayTube.FatherRayTube, nodes);
                        break;
                    }
                }
            }
            else if (currentRayTube.OneRayModels[0].LaunchNode.NodeStyle == NodeStyle.Tx)
            {
                nodes.Insert(0, (Node)currentRayTube.OneRayModels[0].LaunchNode.Clone());
            }
            else
            {
                LogFileManager.ObjLog.debug("求当前节点的父节点时出错！\n");
            }
        }
        private void GetFrontNodesOfAreaSituationNodess(RayTubeModel currentRayTube, List<Node> nodes)
        {
            if (currentRayTube.FatherRayTube != null)
            {
                nodes.Insert(0, (Node)currentRayTube.OneRayModels[0].LaunchNode.Clone());
                this.GetFrontNodesOfAreaSituationNodess(currentRayTube.FatherRayTube, nodes);
            }
            else if (currentRayTube.OneRayModels[0].LaunchNode.NodeStyle == NodeStyle.Tx)
            {
                nodes.Insert(0, (Node)currentRayTube.OneRayModels[0].LaunchNode.Clone());
            }
            else
            {
                LogFileManager.ObjLog.debug("求当前节点的父节点时出错！\n");
            }
        }
        /// <summary>
        /// 根据离散倍数，获取离散的态势节点
        /// </summary>
        /// <param name="rect">当前态势层</param>
        /// <param name="rowDiscretedNum">行离散倍数</param>
        /// <param name="lineDiscretedNum">列离散倍数</param>
        /// <returns>态势节点</returns>
        private List<AreaNode> GetAreaNodesByDiscretedNum(Rectangle rect, int rowDiscretedNum, int lineDiscretedNum)
        {
            List<AreaNode> areaNodes = new List<AreaNode>();
            List<Point> discretedPoints = new List<Point>();
            discretedPoints.AddRange(this.GetDiscretePointsFromRect(rect, rowDiscretedNum, lineDiscretedNum));//将态势层离散成points
            //将point转化为AreaNodes
            for (int i = 0; i < discretedPoints.Count; i++)
            {
                AreaNode virtualRxNode = new AreaNode
                {
                    Position = discretedPoints[i],
                    NodeStyle = NodeStyle.Rx,
                    paths = new List<Path>()
                };
                areaNodes.Add(virtualRxNode);
            }
            return areaNodes;
        }

        private List<List<AreaNode>> GetAreaNodesByDiscretedNums2D(Rectangle rect, int rowDiscretedNum, int lineDiscretedNum)
        {
            List<List<AreaNode>> areaNodes = new List<List<AreaNode>>();
            List <List<Point>> areaPoints = new List<List<Point>>();
            areaPoints = this.GetDiscretePointsFromRect2D(rect, rowDiscretedNum, lineDiscretedNum);
            //将point转化为AreaNodes
            for(int i=0;i<areaPoints.Count;i++)
            {
                List<AreaNode> tempNodes = new List<AreaNode>();
                for(int j=0;j<areaPoints[i].Count;j++)
                {
                    AreaNode virtualRxNode = new AreaNode
                    {
                        Position=areaPoints[i][j],
                        NodeStyle = NodeStyle.Rx,
                        paths = new List<Path>()
                    };
                    tempNodes.Add(virtualRxNode);
                }
                areaNodes.Add(tempNodes);
            }
            return areaNodes;
        }
        /// <summary>
        /// 将矩形离散成点points
        /// </summary>
        /// <param name="rect">矩形</param>
        /// <param name="rowDiscretedNum">行离散倍数</param>
        /// <param name="lineDiscretedNum">列离散倍数</param>
        /// <returns>离散点的链表</returns>
        private List<Point> GetDiscretePointsFromRect(Rectangle rect, int rowDiscretedNum, int lineDiscretedNum)
        {
            List<Point> discretePoints = new List<Point>();//存储离散的点
            Point leftBottomPoint = rect.LeftBottomPoint;//记录矩形的四个点
            Point leftTopPoint = rect.LeftTopPoint;
            Point rightBottomPoint = rect.RightBottomPoint;
            RayInfo rayFromLeftBottomToTop = new RayInfo(leftBottomPoint, new SpectVector(leftBottomPoint, leftTopPoint));//构造一条从左下角到左上角的射线
            SpectVector vectorFromLeftBottomToRightBottom = new SpectVector(leftBottomPoint, rightBottomPoint);//构造一条从左下角到右下角的方向向量
            double lineUnitLength = leftBottomPoint.GetDistance(leftTopPoint) / lineDiscretedNum;//将列离散后每小段的距离
            double rowUnitLength = leftBottomPoint.GetDistance(rightBottomPoint) / rowDiscretedNum;//将行离散后每小段的距离
            Point[] pointsOfLeftEdge = new Point[lineDiscretedNum + 1];
            for (int i = 0; i <= lineDiscretedNum; i++)//等间隔在左边上取点
            {
                pointsOfLeftEdge[i] = rayFromLeftBottomToTop.GetPointOnRayVector(i * lineUnitLength);
            }
            List<Point>[] pointsOfEveryEdge = new List<Point>[lineDiscretedNum + 1];
            for (int j = 0; j <= lineDiscretedNum; j++)
            {
                pointsOfEveryEdge[j] = this.GetPointListAtRayVectorByLength(vectorFromLeftBottomToRightBottom,
                       pointsOfLeftEdge[j], rowUnitLength, rowDiscretedNum);//获得每行的离散点
                pointsOfEveryEdge[j].Insert(0, pointsOfLeftEdge[j]);//将这一行最左边的点插入到队列的最前端
                discretePoints.AddRange(pointsOfEveryEdge[j]);//将每行离散的点添加到整体的列表中
            }
            return discretePoints;
        }
        private List<List<Point>> GetDiscretePointsFromRect2D(Rectangle rect, int rowDiscretedNum, int lineDiscretedNum)
        {
            List<List<Point>> discretePoints = new List<List<Point>>();//存储离散的点
            Point leftBottomPoint = rect.LeftBottomPoint;//记录矩形的三个点
            Point leftTopPoint = rect.LeftTopPoint;
            Point rightBottomPoint = rect.RightBottomPoint;
            RayInfo rayFromLeftBottomToTop = new RayInfo(leftBottomPoint, new SpectVector(leftBottomPoint, leftTopPoint));//构造一条从左下角到左上角的射线
            SpectVector vectorFromLeftBottomToRightBottom = new SpectVector(leftBottomPoint, rightBottomPoint);//构造一条从左下角到右下角的方向向量
            double lineUnitLength = leftBottomPoint.GetDistance(leftTopPoint) / lineDiscretedNum;//将列离散后每小段的距离
            double rowUnitLength = leftBottomPoint.GetDistance(rightBottomPoint) / rowDiscretedNum;//将行离散后每小段的距离
            Point[] pointsOfLeftEdge = new Point[lineDiscretedNum + 1];
            for (int i = 0; i <= lineDiscretedNum; i++)//等间隔在左边上取点
            {
                pointsOfLeftEdge[i] = rayFromLeftBottomToTop.GetPointOnRayVector(i * lineUnitLength);
            }
            List<Point>[] pointsOfEveryEdge = new List<Point>[lineDiscretedNum + 1];
            for (int j = 0; j <= lineDiscretedNum; j++)
            {
                pointsOfEveryEdge[j] = this.GetPointListAtRayVectorByLength(vectorFromLeftBottomToRightBottom,
                       pointsOfLeftEdge[j], rowUnitLength, rowDiscretedNum);//获得每行的离散点
                pointsOfEveryEdge[j].Insert(0, pointsOfLeftEdge[j]);//将这一行最左边的点插入到队列的最前端
                discretePoints.Add(pointsOfEveryEdge[j]);//将每行离散的点添加到整体的列表中
            }
            return discretePoints;
        }
        /// <summary>
        ///获得正二十面体的20个初始射线管
        /// </summary>
        ///  <param name="tx">发射机</param>
        /// <returns></returns>
        private List<RayTubeModel> GetOriginUnitsOfIcosahedron()
        {
            //求正二十面体顶点坐标的方法参考论文：单纯形射线追踪法计算室内场强-王文华
            //注意论文中的XY坐标与我们使用的相反，所以要把x,y的公式反过来
            double thi = 63.4349488229220 * Math.PI / 180;
            double fi = 72 * Math.PI / 180;
            Point[] icosahedronVetices = new Point[12];
            icosahedronVetices[0] = new Point(this.tx.Position.X, this.tx.Position.Y, this.tx.Position.Z + 1);//顶点1
            for (int i = 1; i < 6; i++)//求顶点2-6的坐标
            {
                icosahedronVetices[i] = new Point(this.tx.Position.X + Math.Sin(thi) * Math.Cos(fi * (i - 1)),
                    this.tx.Position.Y + Math.Sin(thi) * Math.Sin(fi * (i - 1)), this.tx.Position.Z + Math.Cos(thi));
            }
            for (int j = 6; j < 11; j++)//求顶点7-11的坐标
            {
                icosahedronVetices[j] = new Point(this.tx.Position.X + Math.Sin(thi) * Math.Cos(fi * (j - 6) + Math.PI / 5),
                    this.tx.Position.Y + Math.Sin(thi) * Math.Sin(fi * (j - 6) + Math.PI / 5), this.tx.Position.Z - Math.Cos(thi));
            }
            icosahedronVetices[11] = new Point(this.tx.Position.X, this.tx.Position.Y, this.tx.Position.Z - 1);//顶点12
            //生产单射线模型
            OneRayModel[] originRayModels = new OneRayModel[12];
            for (int n = 0; n < originRayModels.Length; n++)
            {
                originRayModels[n] = new OneRayModel(this.tx, new RayInfo(this.tx.Position, icosahedronVetices[n]));
            }
            List<RayTubeModel> rayTubeModels = this.GetOriginIcosahedronRayTubeModes(originRayModels);
            return rayTubeModels;
        }



        /// <summary>
        ///获得处理单元及其细分单元中的射线
        /// </summary>
        ///  <param name="originRay">单射线模型</param>
        /// <returns>正二十面体初始射线管模型</returns>
        private List<RayTubeModel> GetOriginIcosahedronRayTubeModes(OneRayModel[] originRay)
        {
            List<RayTubeModel> rayTubeModels = new List<RayTubeModel>();
            //正二十面体上层三角面,共5个
            for (int j = 1; j < 5; j++)
            {
                rayTubeModels.Add(new RayTubeModel(new List<OneRayModel> { originRay[0], originRay[j], originRay[j + 1] }, this.tx.Position));
            }
            rayTubeModels.Add(new RayTubeModel(new List<OneRayModel> { originRay[0], originRay[5], originRay[1] }, this.tx.Position));
            //正二十面体下层三角面，共5个
            for (int k = 6; k < 10; k++)
            {
                rayTubeModels.Add(new RayTubeModel(new List<OneRayModel> { originRay[11], originRay[k], originRay[k + 1] }, this.tx.Position));
            }
            rayTubeModels.Add(new RayTubeModel(new List<OneRayModel> { originRay[11], originRay[10], originRay[6] }, this.tx.Position));
            //正二十面体中层三角面
            for (int n = 1; n < 5; n++)
            {
                rayTubeModels.Add(new RayTubeModel(new List<OneRayModel> { originRay[n], originRay[n + 1], originRay[n + 5] }, this.tx.Position));
            }
            rayTubeModels.Add(new RayTubeModel(new List<OneRayModel> { originRay[5], originRay[1], originRay[10] }, this.tx.Position));
            for (int m = 6; m < 10; m++)
            {
                rayTubeModels.Add(new RayTubeModel(new List<OneRayModel> { originRay[m], originRay[m + 1], originRay[m - 4] }, this.tx.Position));
            }
            rayTubeModels.Add(new RayTubeModel(new List<OneRayModel> { originRay[10], originRay[6], originRay[1] }, this.tx.Position));
            return rayTubeModels;
        }



        /// <summary>
        ///初始阶段细分正二十面体一个三角面，得到多个射线管模型
        /// </summary>
        /// <param name="tx">发射机</param>
        /// <param name="originModel">初始射线管</param>
        /// <param name="tessellationFrequency">镶嵌次数，等于三角形每条边的细分次数</param>
        /// <returns>射线管模型</returns>
        private Stack<RayTubeModel> GetInitialRayTubeModels(Node tx, RayTubeModel originModel, int tessellationFrequency)
        {
            Point vertex1 = originModel.OneRayModels[0].LaunchRay.GetPointOnRayVector(10);//取三角形的顶点
            Point vertex2 = originModel.OneRayModels[1].LaunchRay.GetPointOnRayVector(10);//三角形的顶点
            Point vertex3 = originModel.OneRayModels[2].LaunchRay.GetPointOnRayVector(10);//三角形的顶点
            RayInfo rayFromVertex1To2 = new RayInfo(vertex1, new SpectVector(vertex1, vertex2));//顶点1到2的射线
            SpectVector vectorFromVertex2To3 = new SpectVector(vertex2, vertex3);//顶点2到3的射线
            double unitLength = vertex1.GetDistance(vertex2) / tessellationFrequency;//每段的距离
            Point[] pointOfEdge12 = new Point[tessellationFrequency]; //存放棱12上的点,不包括顶点1
            for (int i = 0; i < tessellationFrequency; i++)//按间隔在棱12上去点
            {
                pointOfEdge12[i] = rayFromVertex1To2.GetPointOnRayVector((i + 1) * unitLength);

            }
            List<Point>[] vertexPoints = new List<Point>[tessellationFrequency + 1];//存放三角形内每条平行边上的点
            vertexPoints[0] = new List<Point> { vertex1 };//把顶点1放到数组第一位
            for (int j = 1; j <= tessellationFrequency; j++)//得到三角形切分后每条边上的点
            {
                vertexPoints[j] = this.GetPointListAtRayVectorByLength(vectorFromVertex2To3, pointOfEdge12[j - 1], unitLength, j);
                vertexPoints[j].Insert(0, pointOfEdge12[j - 1]);
            }
            List<OneRayModel>[] newRays = this.GetDivisionRaysByVertices(tx, tessellationFrequency, vertexPoints);//根据得到的点构造射线
            Stack<RayTubeModel> rayTubeModels = this.GetDivisionModesFromInitialRays(newRays);//得到初始追踪完的三角形处理单元
            return rayTubeModels;
        }

        /// <summary>
        ///得到三角形切分后每条边上的点，但不包括初始端点
        /// </summary>
        /// <param name="vector">从开始端点发出的方向向量</param>
        /// <param name="startPoint">开始端点</param>
        /// <param name="length">长度</param>
        ///  <param name="TessellationFrequency">镶嵌次数，等于三角形每条边的细分次数</param>
        ///  <param name="divideAngle">射线束夹角</param>
        /// <returns></returns>
        private List<Point> GetPointListAtRayVectorByLength(SpectVector vector, Point startPoint, double length, int TessellationFrequency)
        {
            List<Point> vertices = new List<Point>();
            RayInfo lineRay = new RayInfo(startPoint, vector);
            for (int i = 1; i <= TessellationFrequency; i++)//根据端点和方向向量，得到在射线方向上每一个点
            {
                Point temp = lineRay.GetPointOnRayVector(i * length);
                vertices.Add(temp);
            }
            return vertices;
        }

        /// <summary>
        ///得到正二十面体一个三角形初始细分时的射线
        /// </summary>
        /// <param name="tx">发射机</param>
        ///  <param name="tessellationFrequency">镶嵌次数，等于三角形每条边的细分次数</param>
        ///  <param name="vertexPoints">细分三角形点的集合</param>
        /// <returns></returns>
        private List<OneRayModel>[] GetDivisionRaysByVertices(Node tx, int tessellationFrequency, List<Point>[] vertexPoints)
        {
            List<OneRayModel>[] newRays = new List<OneRayModel>[tessellationFrequency + 1];
            for (int m = 0; m < vertexPoints.Length; m++)//遍历所有顶点得到所有发射模型
            {
                newRays[m] = new List<OneRayModel>();
                for (int n = 0; n < vertexPoints[m].Count; n++)
                {
                    newRays[m].Add(new OneRayModel(tx, new RayInfo(tx.Position, new SpectVector(tx.Position, vertexPoints[m][n]))));
                }
            }
            return newRays;
        }

        /// <summary>
        ///将初始三角形上的各条射线组成射线管模型
        /// </summary>
        ///  <param name="oneRayModels">单射线模型</param>
        /// <returns>射线管模型</returns>
        private Stack<RayTubeModel> GetDivisionModesFromInitialRays(List<OneRayModel>[] oneRayModels)
        {
            Stack<RayTubeModel> triangleUnits = new Stack<RayTubeModel>();
            for (int i = 0; i < oneRayModels.Length - 1; i++)//得到正立的三角面射线管
            {
                for (int j = 0; j < oneRayModels[i].Count; j++)
                {
                    RayTubeModel param = new RayTubeModel(new List<OneRayModel>{ oneRayModels[i][j], oneRayModels[i + 1][j],
                        oneRayModels[i + 1][j + 1]}, this.tx.Position);
                    triangleUnits.Push(param);
                }
            }
            if (oneRayModels.Length > 2)//若三角形平分大于2层，得到倒立的三角面射线管
            {
                for (int m = 3; m < oneRayModels.Length; m++)
                {
                    for (int n = 1; n < oneRayModels[m].Count - 1; n++)
                    {
                        RayTubeModel param = new RayTubeModel(new List<OneRayModel>{oneRayModels[m][n], oneRayModels[m - 1][n - 1], 
                            oneRayModels[m - 1][n]}, this.tx.Position);
                        triangleUnits.Push(param);
                    }
                }
            }
            return triangleUnits;
        }


        /// <summary>
        ///获得处理单元及其细分单元中的射线
        /// </summary>
        /// <param name="rayTubeModels">射线管的栈</param>
        /// <param name="ter">地形</param>
        /// <param name="rxBall">接收球</param>
        ///  <param name="buildings">建筑物</param>
        /// <returns></returns>
        private List<Path> GetPathsFromRayTubeModels(Stack<RayTubeModel> rayTubeModels, Terrain ter, ReceiveBall rxBall, City buildings)
        {
            if (rayTubeModels == null || rayTubeModels.Count == 0)
            {
                LogFileManager.ObjLog.error("在追踪处理射线管模型时，输入参数为0或者空");
                return new List<Path>();
            }
            while (rayTubeModels.Count != 0)
            {
                RayTubeModel paramModel = rayTubeModels.Pop();
                paramModel.UpdateTheReveivedFlag(ter, rxBall, buildings);//判断射线管有没有到达接收机
                if (!paramModel.IsReachingRx)//若没有到达接收机
                {                   
                    if (paramModel.HaveTraced)//射线管已完成与地形，建筑物的求交计算
                    {
                        this.HandleTheRayTubeModel(paramModel, rayTubeModels);
                    }
                    else//该处理单元还未进行与地形，建筑物的求交计算
                    {
                        paramModel.TracingThisRayTubeModel(ter, buildings);//射线求交
                        this.HandleTheRayTubeModel(paramModel, rayTubeModels);
                    }
                    //Console.WriteLine(rayTubeModels.Count);
                    //if (rayTubeModels.Count == 605)
                    //{
                    //    int a = 1;
                    //}
                }
            }
            List<Path> paths = this.GetReceivedPaths();
            return paths;

        }
        /// <summary>
        ///对一个射线管进行处理
        /// </summary>
        /// <param name="paramModel">射线管</param>
        ///  <param name="rayTubeModels">射线管的栈</param>
        /// <returns></returns>
        private void HandleTheRayTubeModel(RayTubeModel paramModel, Stack<RayTubeModel> rayTubeModels)
        {
            if (paramModel.JudgeIfThisModelNeedToBeDivided())//若射线管需要进行细分
            {
                List<RayTubeModel> newModels = paramModel.GetDivisionRayTubeModels();//细分后的射线管模型
                for (int i = 0; i < newModels.Count; i++)
                {
                    rayTubeModels.Push(newModels[i]);//加入栈中
                }
            }
            else//射线管不需要细分
            {
                if (paramModel.ReflectionTracingTimes + paramModel.DiffractionTracingTimes >= 4
                    || paramModel.DiffractionTracingTimes >= 1)//若射线管满足截止条件
                { return; }
                else//获取射线管下一阶段的射线管模型
                {
                    List<RayTubeModel> nextModels = paramModel.GetNextRayTubeModels();//下一阶段射线管模型
                    for (int i = 0; i < nextModels.Count; i++)
                    {
                        rayTubeModels.Push(nextModels[i]);//加入栈中
                    }
                }

            }
        }
        /// <summary>
        /// 获取与态势相交的射线管
        /// </summary>
        /// <param name="rayTubeModels">存放射线管的栈</param>
        /// <param name="ter">地形</param>
        /// <param name="reArea">态势区域</param>
        /// <param name="buildings">建筑物</param>
        /// <returns>按态势层的次序存放与态势层相交的射线管</returns>
        private void GetRayTubesCrossWithAreaSituation(List<RayTubeModel>[] tubesCrossWithAreaSituation, Stack<RayTubeModel> rayTubeModels, Terrain ter, ReceiveArea reArea, City buildings)
        {
            if (rayTubeModels == null || rayTubeModels.Count == 0)
            {
                LogFileManager.ObjLog.error("在追踪处理射线管模型时，输入参数为0或者空");
            }
            while (rayTubeModels.Count != 0)
            {

                RayTubeModel paramModel = rayTubeModels.Pop();//从栈中抛出一个射线管进行处理
                if (!paramModel.HaveTraced)//判断射线管是否已完成与地形，建筑物的求交计算过程
                {
                    paramModel.TracingThisRayTubeModel(ter, buildings);//进行射线求交
                }
                //对射线管进行处理,包括是否细分、是否到达态势区域、是否满足截止条件
                this.HandleTheRayTubeModel(paramModel, rayTubeModels, reArea, tubesCrossWithAreaSituation);
            }
        }
        /// <summary>
        ///对一个射线管进行处理
        /// </summary>
        /// <param name="paramModel">射线管</param>
        ///  <param name="rayTubeModels">射线管的栈</param>
        /// <returns></returns>
        private void HandleTheRayTubeModel(RayTubeModel paramModel, Stack<RayTubeModel> rayTubeModels, ReceiveArea reArea, List<RayTubeModel>[] newRayTubes)
        {
            if (paramModel.JudgeIfThisModelNeedToBeDivided())//判断射线管是否需要进行细分
            {
                List<RayTubeModel> newModels = paramModel.GetDivisionRayTubeModels();//细分射线管模型，将细分后的射线管压入栈中
                for (int i = 0; i < newModels.Count; i++)
                {
                    rayTubeModels.Push(newModels[i]);//加入栈中
                }
            }
            else//射线管不需要细分
            {
                //进入此环节的射线管已经完成了与地面和建筑物的求交计算，并且射线管不需要再细分
                //判断射线管有没有到达态势区域
                paramModel.UpdateAreaSituationFlag(reArea);//判断射线管有没有到达态势区域
                if (paramModel.IsReachingArea)//若射线管到达态势区域
                {
                    for (int i = 0; i < paramModel.crossLayerNum.Count; i++)//依次读取当前射线管中crossLayerNum的信息
                    {
                        int num = paramModel.crossLayerNum[i];
                        newRayTubes[num].Add((RayTubeModel)paramModel.Clone());//将射线管添加到对应的newRayTubes中
                    }
                }

                if (paramModel.ReflectionTracingTimes + paramModel.DiffractionTracingTimes >= 4
                      || paramModel.DiffractionTracingTimes >= 2)//判断射线管是否满足截止条件
                {
                    return;
                }
                else//获取射线管下一阶段的射线管模型
                {
                    List<RayTubeModel> nextModels = paramModel.GetNextRayTubeModels();//下一阶段射线管模型
                    for (int i = 0; i < nextModels.Count; i++)
                    {
                        rayTubeModels.Push(nextModels[i]);//加入栈中
                    }
                }

            }
        }
  

        /// <summary>
        ///获取到达接收球的路径
        /// </summary>
        /// <returns>路径</returns>
        private List<Path> GetReceivedPaths()
        {
            List<Path> allPaths = new List<Path>();
            List<Node> nodes = new List<Node>();
            this.GetPathsFromChildNodes(this.tx, allPaths, nodes);
            for (int i = allPaths.Count - 1; i >= 0; i--)
            {
                if (allPaths[i].Rxnum == 0)
                {
                    allPaths.RemoveAt(i);
                    continue;
                }
            }
            this.tx.ChildNodes.Clear();
            return allPaths;
        }

        /// <summary>
        ///根据根节点及其子节点的信息，获得路径
        /// </summary>
        /// <param name="root">根节点</param>
        ///  <param name="allPaths">路径List</param>
        ///  <param name="nodes">节点List</param>
        /// <returns></returns>
        private void GetPathsFromChildNodes(Node root, List<Path> allPaths, List<Node> nodes)
        {
            nodes.Add((Node)root.Clone());
            if (root.ChildNodes.Count != 0)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    this.GetPathsFromChildNodes(root.ChildNodes[i], allPaths, new List<Node>(nodes));
                }
                root.ChildNodes.Clear();
            }
            else
            {
                allPaths.Add(new Path(nodes));
            }
        }

        /// <summary>
        /// 删除重复路径
        /// </summary>
        /// <param name="paths">路径集合</param>
        private void DeleteRepeatedPath(List<Path> paths)
        {
            for (int i = 0; i < paths.Count - 1; i++)
            {
                for (int j = i + 1; j < paths.Count; j++)
                {
                    bool isRepeated = true;
                    if (paths[j].node.Count != paths[i].node.Count)
                    {
                        continue;
                    }
                    else
                    {
                        for (int m = 1; m < paths[j].node.Count - 1; m++)
                        {
                            if (paths[j].node[m].NodeStyle == paths[i].node[m].NodeStyle
                                && Math.Abs(paths[j].node[m].Position.X - paths[i].node[m].Position.X) < 0.000001
                                && Math.Abs(paths[j].node[m].Position.Y - paths[i].node[m].Position.Y) < 0.000001
                                && Math.Abs(paths[j].node[m].Position.Z - paths[i].node[m].Position.Z) < 0.000001)
                            {
                                continue;
                            }
                            else
                            {
                                isRepeated = false;
                                break;
                            }
                        }
                        if (isRepeated == true)
                        {
                            paths.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 计算路径的损耗，延时，相位
        /// </summary>
        private void GetLossAndComponentOFPath(Path midpath)
        {
            midpath.pathloss = 10 * Math.Log10(midpath.node[0].Power / midpath.node[midpath.node.Count - 1].Power);
            midpath.Delay = midpath.GetPathLength() / 300000000;
            midpath.thetaa = ReadUan.GetThetaAngle(midpath.node[0].Position, midpath.node[1].Position);
            midpath.thetab = ReadUan.GetThetaAngle(midpath.node[midpath.node.Count - 2].Position, midpath.node[midpath.node.Count - 1].Position);
            midpath.phia = ReadUan.GetPhiAngle(midpath.node[0].Position, midpath.node[1].Position);
            midpath.phib = ReadUan.GetPhiAngle(midpath.node[midpath.node.Count - 2].Position, midpath.node[midpath.node.Count - 1].Position);

        }

        /// <summary>
        ///根据点类型获得电场值
        /// </summary>
        private void GetEfield(string uan, Node fatherNode, ref Node childNode)
        {
            switch (fatherNode.NodeStyle)
            {
                case NodeStyle.Tx:
                    //直射场强计算
                    childNode.TotalE = DirectEfieldCal.EfieldCal(uan, fatherNode.Power, fatherNode.Frequence, fatherNode.Position, childNode.Position);
                    break;
                case NodeStyle.ReflectionNode:
                    childNode.TotalE = ReflectEfieldCal.ReflectEfield(fatherNode.TotalE, fatherNode.RayIn, childNode.RayIn, fatherNode.ReflectionFace.NormalVector, fatherNode.ReflectionFace.Material.DielectricLayer[0].Conductivity, fatherNode.ReflectionFace.Material.DielectricLayer[0].Permittivity, fatherNode.DistanceToFrontNode, childNode.DistanceToFrontNode, fatherNode.Frequence);
                    break;
                case NodeStyle.DiffractionNode:
                    childNode.TotalE = DiffractEfiledCal.GetDiffractionEField(fatherNode.TotalE, fatherNode.RayIn, fatherNode.DiffractionEdge.AdjacentTriangles[0], fatherNode.DiffractionEdge.AdjacentTriangles[1], fatherNode.Position, childNode.Position, childNode.Frequence);
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        ///根据根节点及其子节点的信息，获得路径
        /// </summary>
        /// <param name="root">根节点</param>
        ///  <param name="allPaths">路径List</param>
        ///  <param name="nodes">节点List</param>
        /// <returns></returns>
        private void CalculatePolarizationComponent()
        {

        }



    }
}
