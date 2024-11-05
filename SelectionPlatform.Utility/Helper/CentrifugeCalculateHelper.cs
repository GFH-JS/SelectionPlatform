using DataClass;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.DTO;
using SelectionPlatform.Utility.Extensions;
using SMARDT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WaterCooled.Centrifugal.Rating;
using McQuay.Engines.Chiller.Core;
using McQuay.Engines.Chiller.Core.Units;
using System.Text.Json;
using Chiller.Core;
using SelectionPlatform.IRepository.ParamComparison;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using static Amazon.Runtime.Internal.CapacityManager;
using Daikin.McQuay.App.Common;
using UnityEngine;

namespace SelectionPlatform.Utility.Helper
{
    public class CentrifugeCalculateHelper
    {
        private IParamComparisonRepository _paramComparisonRepository;
        public CentrifugeCalculateHelper(IParamComparisonRepository paramComparisonRepository)
        {
            _paramComparisonRepository = paramComparisonRepository;  
        }
        public string unit = "SI";
        /// <summary>
        /// 需求冷量变化的时候
        /// </summary>
        /// <param name="projectInputDto"></param>
        /// <returns></returns>
        public ProjectInputDto CapacityToFlow(ProjectInputDto projectInputDto)
        {
            unit = projectInputDto.metricInch;
            var inputdata = projectInputDto.proofData;
            string Capacity = inputdata.capacity.paramValue;
            //流量关联冷量
            inputdata.evaflow.paramValue = DataFormat.EffNumber(MeasurementSystem.CapacityToEvaFlow(Capacity.ToPares(), projectInputDto.metricInch));
            inputdata.condflow.paramValue = DataFormat.EffNumber(MeasurementSystem.CapacityToCondFlow(Capacity.ToPares(), projectInputDto.metricInch));
            //inputdata.gb19577Capacity.paramValue = Capacity;
            //inputdata.gb55015Capacity.paramValue = Capacity;
            //if (inputdata.gb19577.paramValue == "None")
            //{
            //    inputdata.gb19577Capacity.paramValue = "";
            //}
            //else
            //{
            //    inputdata.gb19577Capacity.paramValue = Capacity;
            //}


            //if (inputdata.gb55015.paramValue == "None")
            //{
            //    inputdata.gb55015Capacity.paramValue = "";
            //}
            //else
            //{
            //    inputdata.gb55015Capacity.paramValue = Capacity;
            //}
           

            return CalculateInput(projectInputDto);
        }

        /// <summary>
        /// 根据输入 计算loadpoingdata 界面展示 + GB(计算不展示)
        /// </summary>
        /// <param name="projectInputDto"></param>
        /// <returns></returns>
        public ProjectInputDto CalculateInput(ProjectInputDto projectInputDto_source, CapacityType capacityType = CapacityType.NormalCapacity)
        {
            var projectInputDto = projectInputDto_source;
            if (capacityType != CapacityType.NormalCapacity)
            {
                projectInputDto = CommonHelper.JsonDeepClone(projectInputDto_source);  //第一组数据不要改变
            }

            unit = projectInputDto.metricInch;
            var inputdata = projectInputDto.proofData;
            int PointCount = int.Parse(inputdata.count.paramValue);
            string Capacity = inputdata.capacity.paramValue;    // GB15977||GB55015
            #region 界面GB冷量值联动
            if (inputdata.gb19577.paramValue == "None")
            {
                inputdata.gb19577Capacity.paramValue = "";
            }
            else if (inputdata.gb19577.paramValue == "GB")
            {
                inputdata.gb19577Capacity.paramValue = Capacity;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(inputdata.gb19577Capacity.paramValue))  //初始special值
                {
                    inputdata.gb19577Capacity.paramValue = Capacity;
                }
            }
            if (inputdata.gb55015.paramValue == "None")
            {
                inputdata.gb55015Capacity.paramValue = "";
            }
            else if (inputdata.gb55015.paramValue == "GB")
            {
                inputdata.gb55015Capacity.paramValue = Capacity;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(inputdata.gb55015Capacity.paramValue))  //初始special值
                {
                    inputdata.gb55015Capacity.paramValue = Capacity;
                }
            } 
            #endregion

            if (capacityType == CapacityType.GB15977Capacity)
            {
                Capacity = inputdata.gb19577Capacity.paramValue;
                PointCount = 4;
                //inputdata.count.paramValue = "4";
            }
            if (capacityType == CapacityType.GB55015Capacity)
            {
                Capacity = inputdata.gb55015Capacity.paramValue;
                PointCount = 4;
                //inputdata.count.paramValue = "4";
            }
            if (capacityType == CapacityType.GB15977Capacity || capacityType == CapacityType.GB55015Capacity)
            {
                inputdata.partloadType.paramValue = PointTypeConstants.m_cstrGBConditon;  //GB 工况
            }


           
            string eva_flow = inputdata.evaflow.paramValue;
            string eva_lwt = inputdata.evalwt.paramValue.ToString();
            string eva_ewt = inputdata.evaewt.paramValue.ToString();
            string cond_flow = inputdata.condflow.paramValue;
            string cond_lwt = inputdata.condlwt.paramValue.ToString();
            string cond_ewt = inputdata.condewt.paramValue.ToString();
            string In_VPF_Evaporator_MinPct = inputdata.evaporatorVpfMin.paramValue;
            string In_VPF_Condenser_MinPct = inputdata.condenserVpfMin.paramValue;

            if (capacityType == CapacityType.ConstantCondenserEWT) //EWT 标准工况
            {
                PointCount = 10;
                inputdata.partloadType.paramValue = PointTypeConstants.m_cstrCCEWT;
                inputdata.evaflowEwt.paramValue = FluidCalType.ewt_lwt;
               
                eva_ewt = DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(12, "SI", unit)));
                eva_lwt = DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(7, "SI", unit)));
                cond_ewt = DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(32, "SI", unit)));
                cond_lwt = DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(37, "SI", unit)));

            }

            string partLoadType = inputdata.partloadType.paramValue;
            string eva_flow_ewt = inputdata.evaflowEwt.paramValue;

            if (eva_flow_ewt == FluidCalType.flow_lwt)
            {
                inputdata.condflowEwt.paramValue = FluidCalType.flow_ewt;
            }
            else
            {
                inputdata.condflowEwt.paramValue = FluidCalType.ewt_lwt;
            }


         


            ResultDataTitle resultDataTitle = new ResultDataTitle("");

            if (unit != "SI")
            {
                TitleUnitConvert(resultDataTitle);
            }
            resultDataTitle.PLV_NAME = getIPLVTitle(partLoadType);
            inputdata.partloadDatalists = new List<PartloadDatalist_group>();   //这里根据模式来清除
            PartloadDatalist_group grou1 = new PartloadDatalist_group();
            grou1.name = resultDataTitle.load_point_data;
            grou1.key = "1";
            inputdata.partloadDatalists.Add(grou1);

            for (int k = 1; k < 7; k++) 
            {
                switch (k)
                {
                    case 1:
                        PartloadDatalist partload = new PartloadDatalist();
                        partload.key = $"{grou1.key}.{k}";
                        partload.name = resultDataTitle.percofFullLoad;
                        for (int i = 1; i <= PointCount; i++)
                        {
                            partload.GetType().GetProperty($"k{i}")?.SetValue(partload,DataFormat.EffNumberForTemp((Convert.ToInt32(100.0 / PointCount * (PointCount - i + 1)).ToString())));
                            //partload.dataList.Add(DataFormat.EffNumberForTemp((Convert.ToInt32(100.0 / PointCount * (PointCount - i + 1)).ToString())));
                        }
                        grou1.children.Add(partload);
                        break;
                    case 2:
                        PartloadDatalist partload2 = new PartloadDatalist();
                        partload2.key = $"{grou1.key}.{k}";
                        partload2.name = resultDataTitle.capacity;
                        for (int i = 1; i <= PointCount; i++)
                        {
                            partload2.GetType().GetProperty($"k{i}")?.SetValue(partload2, DataFormat.EffNumber((Convert.ToDouble(Capacity.ToString().Trim()) / PointCount * (PointCount - i + 1))));
                            //partload2.dataList.Add(DataFormat.EffNumber((Convert.ToDouble(Capacity.ToString().Trim()) / PointCount * (PointCount - i + 1))));
                        }
                        grou1.children.Add(partload2);
                        break;
                    case 3:
                        PartloadDatalist partload3 = new PartloadDatalist();
                        partload3.key = $"{grou1.key}.{k}";

                        partload3.name = resultDataTitle.evaewt;
                        if (eva_flow_ewt == FluidCalType.flow_lwt) partload3.name = resultDataTitle.evaflow;
                        if (partLoadType == PointTypeConstants.m_cstrGBConditon)
                        {
                            partload3.name = resultDataTitle.evaflow; //FLOW+LWT
                        }
                        if (partLoadType == PointTypeConstants.m_cstrAHRI)
                        {
                            partload3.name = resultDataTitle.evaewt; //EWT+LWT
                        }
                        double minflow = 0;
                        if ((partLoadType != PointTypeConstants.m_cstrAHRI && eva_flow_ewt == FluidCalType.flow_lwt)
                            || partLoadType == PointTypeConstants.m_cstrUserDef || partLoadType == PointTypeConstants.m_cstrGBConditon)
                        {
                            //partload3.name = resultDataTitle.evaflow;
                            for (int i = 1; i <= PointCount; i++)
                            {
                                if (partLoadType == PointTypeConstants.m_cstrVPF || partLoadType == PointTypeConstants.m_cstrVPFAHRICR)
                                {
                                    if (minflow > getEvapFlow(i,PointCount,float.Parse(eva_flow),Capacity,In_VPF_Evaporator_MinPct, partLoadType).ToPares())
                                    {
                                        partload3.GetType().GetProperty($"k{i}")?.SetValue(partload3, minflow.ToString());
                                        //partload3.dataList.Add(minflow.ToString());
                                    }
                                    else 
                                    {
                                        partload3.GetType().GetProperty($"k{i}")?.SetValue(partload3, getEvapFlow(i, PointCount, float.Parse(eva_flow), Capacity, In_VPF_Evaporator_MinPct, partLoadType));
                                        //partload3.dataList.Add(getEvapFlow(i, PointCount, float.Parse(eva_flow), Capacity, In_VPF_Evaporator_MinPct, partLoadType));
                                    }
                                }
                                else
                                {
                                    partload3.GetType().GetProperty($"k{i}")?.SetValue(partload3, getEvapFlow(i, PointCount, float.Parse(eva_flow), Capacity, In_VPF_Evaporator_MinPct, partLoadType));
                                    //partload3.dataList.Add(getEvapFlow(i, PointCount, float.Parse(eva_flow), Capacity, In_VPF_Evaporator_MinPct, partLoadType));
                                }
                            }
                        }
                        else
                        {
                            //partload3.name = resultDataTitle.evaewt;
                            for (int i = 1; i <= PointCount; i++)
                            {
                                if (partLoadType == PointTypeConstants.m_cstrAHRI && i > 1)
                                {
                                    partload3.GetType().GetProperty($"k{i}")?.SetValue(partload3, getEvapEWT(1, PointCount, float.Parse(eva_ewt), partLoadType)); //默认取第一个点的数据
                                }
                                else
                                {
                                    partload3.GetType().GetProperty($"k{i}")?.SetValue(partload3, getEvapEWT(i, PointCount, float.Parse(eva_ewt), partLoadType));
                                }
                            }    
                        }
                        grou1.children.Add(partload3);
                        break;
                    case 4:
                        PartloadDatalist partload4 = new PartloadDatalist();
                        partload4.key = $"{grou1.key}.{k}";
                        partload4.name = resultDataTitle.evalwt;
                        for (int i = 1; i <= PointCount; i++)
                        {
                            partload4.GetType().GetProperty($"k{i}")?.SetValue(partload4, getEvapLWT(i, PointCount, float.Parse(eva_lwt), partLoadType));
                            //partload4.dataList.Add(getEvapLWT(i, PointCount, float.Parse(eva_lwt), partLoadType));
                        }
                        grou1.children.Add(partload4);
                        break;
                    case 5:
                        PartloadDatalist partload5 = new PartloadDatalist();
                        partload5.key = $"{grou1.key}.{k}";
                        partload5.name = resultDataTitle.condlwt;
                        if (eva_flow_ewt == FluidCalType.flow_lwt) partload5.name = resultDataTitle.condflow;

                        if (partLoadType == PointTypeConstants.m_cstrGBConditon)
                        {
                            partload5.name = resultDataTitle.condflow; //FLOW+LWT
                        }
                        if (partLoadType == PointTypeConstants.m_cstrAHRI)
                        {
                            partload5.name = resultDataTitle.condlwt; //EWT+LWT
                        }

                        double minflow2 = 0;
                        if ((partLoadType != PointTypeConstants.m_cstrAHRI && eva_flow_ewt == FluidCalType.flow_lwt)
                            || partLoadType == PointTypeConstants.m_cstrUserDef || partLoadType == PointTypeConstants.m_cstrGBConditon)
                        {
                            //partload5.name = resultDataTitle.condflow;
                            for (int i = 1; i <= PointCount; i++)
                            {
                                if (partLoadType == PointTypeConstants.m_cstrVPF || partLoadType == PointTypeConstants.m_cstrVPFAHRICR)
                                {
                                    if (minflow2 > getCondFlow(i, PointCount, float.Parse(cond_flow), Capacity, In_VPF_Condenser_MinPct, partLoadType).ToPares())
                                    {
                                        partload5.GetType().GetProperty($"k{i}")?.SetValue(partload5, minflow2.ToString());
                                        //partload5.dataList.Add(minflow2.ToString());
                                    }
                                    else
                                    {
                                        partload5.GetType().GetProperty($"k{i}")?.SetValue(partload5,getCondFlow(i, PointCount, float.Parse(cond_flow), Capacity, In_VPF_Condenser_MinPct, partLoadType));
                                        //partload5.dataList.Add(getCondFlow(i, PointCount, float.Parse(cond_flow), Capacity, In_VPF_Condenser_MinPct, partLoadType));
                                    }
                                }
                                else
                                {
                                    partload5.GetType().GetProperty($"k{i}")?.SetValue(partload5, getCondFlow(i, PointCount, float.Parse(cond_flow), Capacity, In_VPF_Condenser_MinPct, partLoadType));
                                    //partload5.dataList.Add(getCondFlow(i, PointCount, float.Parse(cond_flow),Capacity,In_VPF_Condenser_MinPct, partLoadType));
                                }
                            }
                        }
                        else
                        {
                            //partload5.name = resultDataTitle.condlwt;
                            for (int i = 1; i <= PointCount; i++)
                            {
                              
                                if (partLoadType == PointTypeConstants.m_cstrAHRI && i > 1)
                                {
                                    partload5.GetType().GetProperty($"k{i}")?.SetValue(partload5, getCondLWT(1, PointCount, float.Parse(cond_lwt), partLoadType)); //默认取第一个点的数据
                                }
                                else
                                {
                                    partload5.GetType().GetProperty($"k{i}")?.SetValue(partload5, getCondLWT(i, PointCount, float.Parse(cond_lwt), partLoadType));
                                }
                            }
                        }
                        grou1.children.Add(partload5);
                        break;
                    case 6:
                        PartloadDatalist partload6 = new PartloadDatalist();
                        partload6.key = $"{grou1.key}.{k}";
                        partload6.name = resultDataTitle.condewt;
                        for (int i = 1; i <= PointCount; i++)
                        {
                            partload6.GetType().GetProperty($"k{i}")?.SetValue(partload6, getCondEWT(i, PointCount, float.Parse(cond_ewt), cond_ewt, partLoadType));
                            //partload6.dataList.Add(getCondEWT(i, PointCount, float.Parse(cond_ewt),cond_ewt,partLoadType));
                        }
                        grou1.children.Add(partload6);
                        break;
                    default:
                        break;
                }
            }

            return projectInputDto;
        }


        /// <summary>
        /// Rating 计算
        /// </summary>
        /// <param name="projectInputDto"></param>
        /// <returns></returns>
        public string RatingData(ProjectInputDto projectInputDto)
        {
            #region 计算输入范围
            //var V1 = MeasurementSystem.CapacityConvertor(0, "SI", "IP"); ///0
            //V1 = MeasurementSystem.CapacityConvertor(10000, "SI", "IP");  ///2843.4513499313593

            //V1 = MeasurementSystem.FlowConvertor(0, "SI", "IP");  ///0
            //V1 = MeasurementSystem.FlowConvertor(1000, "SI", "IP");  ///15850.323141488903

            //V1 = MeasurementSystem.TempConvertor(2, "SI", "IP");  ///35.6
            //V1 = MeasurementSystem.TempConvertor(25, "SI", "IP");  ///77

            //V1 = MeasurementSystem.TempConvertor(10, "SI", "IP");  ///50
            //V1 = MeasurementSystem.TempConvertor(40, "SI", "IP");  ///104

            //V1 = MeasurementSystem.TempConvertor(2, "SI", "IP");  ///35.6
            //V1 = MeasurementSystem.TempConvertor(20, "SI", "IP");  ///68


            //V1 = MeasurementSystem.FoulConvertor(0, "SI", "IP");  ///0
            //V1 = MeasurementSystem.FoulConvertor(0.3522, "SI", "IP");  ///0.00199988387597244

            //return projectInputDto; 
            #endregion
            try
            {
                unit = projectInputDto.metricInch;
                var _proofdata = projectInputDto.proofData;
                RatingEngineNew ratingEngineNew = new RatingEngineNew();
                RatingInputModel ratingInputModel = GenerateRatingInputData(projectInputDto,CapacityType.NormalCapacity);

                List<ChillerOutputList> normalRatingRe = null;
                if (projectInputDto.modelType == (int)ModelType.Centrifuge)
                {
                    #region CESHIZHI 
                    //ratingInputModel.Capacity = "3600,2700,1800,900";
                    //ratingInputModel.ChillerModelCode = "WSC126MBJN0F/E3912-RH-2/C3612-HK-2";
                    //ratingInputModel.ChillerType = "WSC";
                    //ratingInputModel.CondDesignPressure = 1;
                    //ratingInputModel.CondEWT = "32,32,32,32";
                    //ratingInputModel.CondFlow = "252.0,252.0,252.0,252.0";
                    //ratingInputModel.CondFluidType = "Water";
                    //ratingInputModel.CondFoul = 0.044f;
                    //ratingInputModel.CondGlycolQty = 0;
                    //ratingInputModel.CondLWT = "37,37,37,37";
                    //ratingInputModel.CondVPFMinPct = 70;
                    //ratingInputModel.CustomSelectionType = "";
                    //ratingInputModel.EvapDesignPressure = 1;
                    //ratingInputModel.EvapEWT = "12,12,12,12";
                    //ratingInputModel.EvapFlow = "201.6,201.6,201.6,201.6";
                    //ratingInputModel.EvapFluidType = "Water";
                    //ratingInputModel.EvapFoul = 0.018f;
                    //ratingInputModel.EvapGlycolQty = 0;
                    //ratingInputModel.EvapLWT = "7,7,7,7";
                    //ratingInputModel.EvapVPFMinPct = 50;
                    //ratingInputModel.FluidCalType = 2;
                    //ratingInputModel.HeatRecoverCapacity = "";
                    //ratingInputModel.HotByPass = "N";
                    //ratingInputModel.IsAHRICertification = false;
                    //ratingInputModel.IsCOPCurve = false;
                    //ratingInputModel.IsCustomSelection = false;
                    //ratingInputModel.IsSCFChillerSystem = false;
                    //ratingInputModel.LowLift = false;
                    //ratingInputModel.MeasurementSystem = "SI";
                    //ratingInputModel.OilCooler = "WOC";
                    //ratingInputModel.OrderDetermination = false;
                    //ratingInputModel.Percentage = "";
                    //ratingInputModel.PointCount = 4;
                    //ratingInputModel.PointType = 6;
                    //ratingInputModel.Power = "50/3/380";
                    //ratingInputModel.Qty = "1";
                    //ratingInputModel.RegionalStandard = "None";
                    //ratingInputModel.RegionalStandard50189 = "None";
                    //ratingInputModel.ReportItems = "";
                    //ratingInputModel.RunningMode = "Cool model";
                    //ratingInputModel.StarterMountingType = "";
                    //ratingInputModel.StartType = "SS";
                    //ratingInputModel.StartTypeDown = "";
                    //ratingInputModel.StrPointType = "";
                    //ratingInputModel.Valve = "";
                    //ratingInputModel.VFDOption = "";
                    #endregion
                    normalRatingRe = ratingEngineNew.RunRating(ratingInputModel, false, false);
                }
                else
                {
                    normalRatingRe = ratingEngineNew.RunRating(ratingInputModel, false, false); //此处后期可能出现模块机selection模式要修改
                }

                #region ConstantCondenserEWT
                var CCEWT_INTPU = CalculateInput(projectInputDto, CapacityType.ConstantCondenserEWT);
                RatingInputModel ccewt_ratingInputModel = GenerateRatingInputData(CCEWT_INTPU, CapacityType.ConstantCondenserEWT); 
                #endregion


                var checkresult = checkOutputList(projectInputDto, normalRatingRe);
                if (!string.IsNullOrEmpty(checkresult)) {
                    return checkresult;
                }

                int ratingResultPointCount = normalRatingRe.Count();

                ResultDataTitle resultDataTitle = new ResultDataTitle("");
                if (unit != "SI")
                {
                    TitleUnitConvert(resultDataTitle);
                }
                resultDataTitle.PLV_NAME = getIPLVTitle(_proofdata.partloadType.paramValue);

                List<Chiller.Core.ChillerOutputList> gb15977Re = null;
                List<Chiller.Core.ChillerOutputList> gb55015Re = null;
                if (_proofdata.gb19577.paramValue != "None")
                {
                    var gb15977PorjInput = CalculateInput(projectInputDto,CapacityType.GB15977Capacity);
                    var gb15977RatingInput = GenerateRatingInputData(gb15977PorjInput, CapacityType.GB15977Capacity);
                    gb15977Re = ratingEngineNew.RunRating(gb15977RatingInput, false,false);
                }

                if (_proofdata.gb55015.paramValue != "None")
                {
                    var gb55015ProjInput = CalculateInput(projectInputDto, CapacityType.GB55015Capacity);
                    var gb55015RatingInput = GenerateRatingInputData(gb55015ProjInput, CapacityType.GB55015Capacity);
                    gb55015Re = ratingEngineNew.RunRating(gb55015RatingInput, false, false);
                }

               
                _proofdata.partloadDatalists.AddRange(GenerateEmptyDataList(ratingInputModel.MeasurementSystem, resultDataTitle, _proofdata.gb19577.paramValue == "None" ? false:true, _proofdata.gb55015.paramValue == "None" ? false : true));
              
                #region param 
                var g2_capacity = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.acapacity).FirstOrDefault();
                var g2_power = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.inputpower).FirstOrDefault();
                var g2_efficiency = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.COP).FirstOrDefault();
                var g2_NPLV = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.PLV_NAME).FirstOrDefault();
                var g2_RLA = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.RLA).FirstOrDefault();

                var g3_evaewt = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.evadata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.evaewt).FirstOrDefault();
                var g3_evalwt = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.evadata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.evalwt).FirstOrDefault();
                var g3_evaflow = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.evadata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.evaflow).FirstOrDefault();
                var g3_evapressuredrop = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.evadata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.predrop).FirstOrDefault();

                var g4_condewt = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.conddata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.condewt).FirstOrDefault();
                var g4_condlwt = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.conddata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.condlwt).FirstOrDefault();
                var g4_condflow = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.conddata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.condflow).FirstOrDefault();
                var g4_condpressuredrop = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.conddata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.predrop).FirstOrDefault();

                var g5_mocp = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.eledata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.MOCP).FirstOrDefault();
                var g5_powerfac = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.eledata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.powerfactor).FirstOrDefault();
                var g5_outputamps = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.eledata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.outputAmps).FirstOrDefault();

                var g6_evatemp = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.servicedata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.evatemp).FirstOrDefault();
                var g6_evapressure = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.servicedata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.evapressure).FirstOrDefault();
                var g6_evawater = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.servicedata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.evawater).FirstOrDefault();
                var g6_condtemp = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.servicedata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.condtemp).FirstOrDefault();
                var g6_condpressure = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.servicedata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.condpressure).FirstOrDefault();
                var g6_condwater = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.servicedata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.condwater).FirstOrDefault();
                var g6_suprehead = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.servicedata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.superheat).FirstOrDefault();
                var g6_subcooling = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.servicedata).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.subcooling).FirstOrDefault();

                var g7_msg = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.mes).FirstOrDefault()?.children.Where(p => p.name == "mes").FirstOrDefault();

                int k = 1;
                foreach (var item in normalRatingRe)
                {
                    
                    #region G2 result
                    ///capacity
                    var prop = g2_capacity?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g2_capacity, DataFormat.EffNumber(MeasurementSystem.CapacityConvertor(item.Capacity.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));//Convert.ChangeType(item.Capacity.GetValueInBaseUnitsOfMeasureAsDouble(), prop.PropertyType)
                    }
                    ///power
                    prop = g2_power?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g2_power, DataFormat.EffNumber(item.PowerInput.GetValueInBaseUnitsOfMeasureAsDouble()));
                    }
                    ///cop
                    prop = g2_efficiency?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g2_efficiency, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(item.Efficiency.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    ///nplv
                    prop = g2_NPLV?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        if (k ==1)
                        {
                            if (unit == "SI")
                                prop.SetValue(g2_NPLV, DataFormat.EffNumber(item.IPLV_SI));
                            else
                                prop.SetValue(g2_NPLV, DataFormat.EffNumber(item.IPLV_IP));
                        }
                        else
                        {
                            prop.SetValue(g2_NPLV, "-");
                        }
                      

                    }
                    ///RLA
                    prop = g2_RLA?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g2_RLA, DataFormat.EffNumber(item.RLA.GetValueInBaseUnitsOfMeasureAsDouble()));
                    }
                    #endregion
                    #region G3 eva data
                    ///evaewt
                    prop = g3_evaewt?.GetType().GetProperty($"k{k}");//蒸发器进水温度
                    if (prop != null)
                    {
                        prop.SetValue(g3_evaewt, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(item.EvaporatorTin.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit))));
                    }
                    ///evalwt
                    prop = g3_evalwt?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g3_evalwt, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(item.EvaporatorTout.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit))));
                    }
                    ///evaflow
                    prop = g3_evaflow?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g3_evaflow, DataFormat.EffNumber(MeasurementSystem.FlowConvertor(item.EvaporatorFlowrate.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    ///evapressuredrop
                    prop = g3_evapressuredrop?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g3_evapressuredrop, DataFormat.EffNumber(MeasurementSystem.PressureDropConvertor(item.EvaporatorPressureDrop.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    #endregion
                    #region G4 cond data
                    ///condewt
                    prop = g4_condewt?.GetType().GetProperty($"k{k}");//冷凝器器进水温度
                    if (prop != null)
                    {
                        prop.SetValue(g4_condewt, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(item.CondenserTin.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit))));
                    }
                    ///condlwt
                    prop = g4_condlwt?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g4_condlwt, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(item.CondenserTout.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit))));
                    }
                    ///condflow
                    prop = g4_condflow?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g4_condflow, DataFormat.EffNumber(MeasurementSystem.FlowConvertor(item.CondenserFlowrate.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    ///condpressuredrip
                    prop = g4_condpressuredrop?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g4_condpressuredrop, DataFormat.EffNumber(MeasurementSystem.PressureDropConvertor(item.CondenserPressureDrop.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    #endregion
                    #region G5 eledata
                    ///MOCP
                    prop = g5_mocp?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g5_mocp, DataFormat.EffNumber(item.MOCP.GetValueInBaseUnitsOfMeasureAsDouble()));
                    }
                    ///powerfac
                    prop = g5_powerfac?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        if (k == 1)
                        {
                            prop.SetValue(g5_powerfac, DataFormat.EffNumber(item.PowerFactor));
                        }
                        else
                        {
                            prop.SetValue(g5_powerfac, "-");
                        }
                    }
                    ///outputamps
                    prop = g5_outputamps?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        if (ratingInputModel.StartType != "VFD")
                        {
                            prop.SetValue(g5_outputamps, "-");
                        }
                        else
                        {
                            prop.SetValue(g5_outputamps, DataFormat.EffNumber(item.OutputAmps.GetValueInBaseUnitsOfMeasureAsDouble()));
                        }
                       
                    }
                    #endregion
                    #region G6 servicedata
                    ///eva temp
                    prop = g6_evatemp?.GetType().GetProperty($"k{k}");//蒸发器蒸发温度
                    if (prop != null)
                    {
                        prop.SetValue(g6_evatemp, DataFormat.EffNumber(MeasurementSystem.TempConvertor(item.EvaporatorTemp.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    ///eva pressure
                    prop = g6_evapressure?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g6_evapressure, DataFormat.EffNumber(MeasurementSystem.PressureConvertor(item.EvaporatorPressure.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    ///eva water
                    prop = g6_evawater?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g6_evawater, DataFormat.EffNumber(MeasurementSystem.VelocityConvertor(item.EvaporatorWaterVel.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    ///cond temp
                    prop = g6_condtemp?.GetType().GetProperty($"k{k}");//
                    if (prop != null)
                    {
                        prop.SetValue(g6_condtemp, DataFormat.EffNumber(MeasurementSystem.TempConvertor(item.CondenserTemp.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    ///cond pressure
                    prop = g6_condpressure?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g6_condpressure, DataFormat.EffNumber(MeasurementSystem.PressureConvertor(item.CondenserPressure.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    ///cond water
                    prop = g6_condwater?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g6_condwater, DataFormat.EffNumber(MeasurementSystem.VelocityConvertor(item.CondenserWaterVel.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                    }
                    ///superheat
                    prop = g6_suprehead?.GetType().GetProperty($"k{k}"); //过热度
                    if (prop != null)
                    {
                        prop.SetValue(g6_suprehead, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.DeltaTempConvertor(item.SuperheatingTemp.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit))));
                    }
                    ///subcooling
                    prop = g6_subcooling?.GetType().GetProperty($"k{k}"); //过冷度
                    if (prop != null)
                    {
                        prop.SetValue(g6_subcooling, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.DeltaTempConvertor(item.SupercoolingTemp.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit))));
                    }
                    #endregion

                    #region errmsg
                    prop = g7_msg?.GetType().GetProperty($"k{k}");
                    if (prop != null)
                    {
                        prop.SetValue(g7_msg, item.ErrorMessage);
                    }
                    #endregion
                    k++;
                }

                bool hasgb19577 = false;
                k = 1;
                //GB19577 PARAM
                if (_proofdata.gb19577.paramValue != "None")
                {
                    var gb19577title= _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.gb15977title).FirstOrDefault();  
                    var gb19577_capacity = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.acapacity).ToList()[1];
                    var gb19577_power = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.inputpower).ToList()[1];
                    var gb19577_efficiency = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.COP).ToList()[1];
                    var gb19577_loadP75 = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.load_P75).FirstOrDefault();
                    var gb19577_loadP50 = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.load_P50).FirstOrDefault();
                    var gb19577_loadP25 = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.load_P25).FirstOrDefault();
                    var gb19577_IPLV_GB = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == ResultDataTitle.IPLVGB).ToList()[0];
                    var gb19577_RLA = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.RLA).ToList()[1];
                    hasgb19577 = true;

                    if (gb15977Re != null)
                    {
                        double outputcop = 0;
                        ///19577 capacity
                        var prop = gb19577_capacity?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {
                            prop.SetValue(gb19577_capacity, DataFormat.EffNumber(MeasurementSystem.CapacityConvertor(gb15977Re[0].Capacity.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));//Convert.ChangeType(item.Capacity.GetValueInBaseUnitsOfMeasureAsDouble(), prop.PropertyType)
                        }
                        ///19577 inputpower
                        prop = gb19577_power?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {                     
                            prop.SetValue(gb19577_power, DataFormat.EffNumber(gb15977Re[0].PowerInput.GetValueInBaseUnitsOfMeasureAsDouble()));
                        }
                        ///19577 efficiency
                        prop = gb19577_efficiency?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {
                            prop.SetValue(gb19577_efficiency, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(gb15977Re[0].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                        }
                        ///19577 efficiency 75
                        prop = gb19577_loadP75?.GetType().GetProperty($"k{k}");
                        if (prop != null && gb15977Re.Count>=2)
                        {
                            if (gb15977Re[1].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat() != 0)
                            {
                                if (Math.Abs(gb15977Re[1].PercentageOfLoad - 0.75) > 0.001)//20201116 XST解决GB standard时有的点无法rating的情况
                                {
                                    outputcop = Daikin.McQuay.Chiller.WaterCooled.Centrifugal.Rating.RatingForm.COPConvert(0.75, gb15977Re[1].PercentageOfLoad, gb15977Re[1].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat());

                                }
                                else
                                    outputcop = gb15977Re[1].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat();
                            }
                            prop.SetValue(gb19577_loadP75, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(outputcop, "SI", unit)));
                        }
                        ///19577 efficiency 50
                        prop = gb19577_loadP50?.GetType().GetProperty($"k{k}");
                        if (prop != null && gb15977Re.Count >= 3)
                        {
                            if (gb15977Re[2].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat() != 0)
                            {
                                if (Math.Abs(gb15977Re[2].PercentageOfLoad - 0.5) > 0.001)//20201116 XST解决GB standard时有的点无法rating的情况
                                {
                                    outputcop = Daikin.McQuay.Chiller.WaterCooled.Centrifugal.Rating.RatingForm.COPConvert(0.5, gb15977Re[2].PercentageOfLoad, gb15977Re[2].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat());

                                }
                                else
                                    outputcop = gb15977Re[2].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat();
                            }
                            prop.SetValue(gb19577_loadP50, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(outputcop, "SI", unit)));
                        }
                        ///19577 efficiency 25
                        prop = gb19577_loadP25?.GetType().GetProperty($"k{k}");
                        if (prop != null && gb15977Re.Count >= 4)
                        {
                            if (gb15977Re[3].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat() != 0)
                            {
                                if (Math.Abs(gb15977Re[3].PercentageOfLoad - 0.25) > 0.001) //20201116 XST解决GB standard时有的点无法rating的情况
                                {
                                    outputcop = Daikin.McQuay.Chiller.WaterCooled.Centrifugal.Rating.RatingForm.COPConvert(0.25, gb15977Re[3].PercentageOfLoad, gb15977Re[3].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat());

                                }
                                else
                                    outputcop = gb15977Re[3].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat();
                            }
                            prop.SetValue(gb19577_loadP25, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(outputcop, "SI", unit)));
                        }
                        ///19577 IPLV_GB
                        prop = gb19577_IPLV_GB?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {
                            if (unit == "SI")
                                prop.SetValue(gb19577_IPLV_GB, DataFormat.EffNumber(gb15977Re[0].IPLV_SI));
                            else
                                prop.SetValue(gb19577_IPLV_GB, DataFormat.EffNumber(gb15977Re[0].IPLV_IP));

                        }
                        ///19577 RLA
                        prop = gb19577_RLA?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {
                            prop.SetValue(gb19577_RLA, DataFormat.EffNumber(gb15977Re[0].RLA.GetValueInBaseUnitsOfMeasureAsDouble()));
                        }

                        var grate = CalculationEfficiency(gb19577_capacity.k1, float.Parse(gb19577_efficiency.k1), double.Parse(gb19577_IPLV_GB.k1), unit);
                        if (grate != 0)
                        {
                            gb19577title.k1 = "pass";
                        }
                        else
                        {
                            gb19577title.k1 = "Does not Comply. The unit fails to rate at GB conditions";
                            gb19577_capacity.k1 = "-";
                            gb19577_power.k1 = "-";
                            gb19577_efficiency.k1 = "-";
                            gb19577_loadP75.k1 = "-";
                            gb19577_loadP50.k1 = "-";
                            gb19577_loadP25.k1 = "-";
                            gb19577_IPLV_GB.k1 = "-";
                            gb19577_RLA.k1 = "-";
                        }
                    }
                }

                //GB55015 PARAM
                if (_proofdata.gb55015.paramValue != "None")
                {
                    var gb55015title = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.gb55015title).FirstOrDefault();
                    int index_55015 = hasgb19577 ? 2 : 1;
                    var gb55015_capacity = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.acapacity).ToList()[index_55015];
                    var gb55015_power = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.inputpower).ToList()[index_55015];
                    var gb55015_efficiency = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.COP).ToList()[index_55015];
                    var gb55015_loadP75 = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.load_P75).ToList()[index_55015 -1];
                    var gb55015_loadP50 = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.load_P50).ToList()[index_55015 - 1];
                    var gb55015_loadP25 = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.load_P25).ToList()[index_55015 - 1];
                    var gb55015_IPLV_GB = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == ResultDataTitle.IPLVGB).ToList()[index_55015-1];
                    var gb55015_RLA = _proofdata.partloadDatalists.Where(g => g.name == resultDataTitle.result).FirstOrDefault()?.children.Where(p => p.name == resultDataTitle.RLA).ToList()[index_55015];


                    if (gb55015Re != null)
                    {
                        double outputcop =0;
                        ///55015 capacity
                        var prop = gb55015_capacity?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {
                            prop.SetValue(gb55015_capacity, DataFormat.EffNumber(MeasurementSystem.CapacityConvertor(gb55015Re[0].Capacity.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                        }
                        ///55015 inputpower
                        prop = gb55015_power?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {
                            prop.SetValue(gb55015_power, DataFormat.EffNumber(gb55015Re[0].PowerInput.GetValueInBaseUnitsOfMeasureAsDouble()));
                        }
                        ///55015 efficiency
                        prop = gb55015_efficiency?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {
                            prop.SetValue(gb55015_efficiency, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(gb55015Re[0].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                        }
                        ///55015 efficiency 75
                        prop = gb55015_loadP75?.GetType().GetProperty($"k{k}");
                        if (prop != null && gb55015Re.Count >= 2)
                        {
                            if (gb55015Re[1].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat() != 0)
                            {
                                if (Math.Abs(gb55015Re[1].PercentageOfLoad - 0.75) > 0.001)//20201116 XST解决GB standard时有的点无法rating的情况
                                {
                                    outputcop = Daikin.McQuay.Chiller.WaterCooled.Centrifugal.Rating.RatingForm.COPConvert(0.75, gb55015Re[1].PercentageOfLoad, gb55015Re[1].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat());

                                }
                                else
                                    outputcop = gb55015Re[1].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat();

                                prop.SetValue(gb55015_loadP75, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(outputcop, "SI", unit)));
                            }
                            
                
                        }
                        ///55015 efficiency 50
                        prop = gb55015_loadP50?.GetType().GetProperty($"k{k}");
                        if (prop != null && gb55015Re.Count >= 3)
                        {
                            if (gb55015Re[2].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat() != 0)
                            {
                                if (Math.Abs(gb55015Re[2].PercentageOfLoad - 0.5) > 0.001)
                                {
                                    outputcop = Daikin.McQuay.Chiller.WaterCooled.Centrifugal.Rating.RatingForm.COPConvert(0.5, gb55015Re[2].PercentageOfLoad, gb55015Re[2].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat());

                                }
                                else
                                    outputcop = gb55015Re[2].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat();
                                prop.SetValue(gb55015_loadP50, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(outputcop, "SI", unit)));
                            }
                           
                        }
                        ///55015 efficiency 25
                        prop = gb55015_loadP25?.GetType().GetProperty($"k{k}");
                        if (prop != null && gb55015Re.Count >= 4)
                        {
                            if (gb55015Re[3].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat() != 0)
                            {
                                if (Math.Abs(gb55015Re[3].PercentageOfLoad - 0.25) > 0.001)
                                {
                                    outputcop = Daikin.McQuay.Chiller.WaterCooled.Centrifugal.Rating.RatingForm.COPConvert(0.25, gb55015Re[3].PercentageOfLoad, gb55015Re[3].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat());

                                }
                                else
                                    outputcop = gb55015Re[3].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat();
                                prop.SetValue(gb55015_loadP25, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(outputcop, "SI", unit)));
                            }
                           
                        }
                        ///55015 IPLV_GB
                        prop = gb55015_IPLV_GB?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {
                            if (unit == "SI")
                                prop.SetValue(gb55015_IPLV_GB, DataFormat.EffNumber(gb55015Re[0].IPLV_SI));
                            else
                                prop.SetValue(gb55015_IPLV_GB, DataFormat.EffNumber(gb55015Re[0].IPLV_IP));
                        }
                        ///55015 RLA
                        prop = gb55015_RLA?.GetType().GetProperty($"k{k}");
                        if (prop != null)
                        {
                            prop.SetValue(gb55015_RLA, DataFormat.EffNumber(gb55015Re[0].RLA.GetValueInBaseUnitsOfMeasureAsDouble()));
                        }

                        var grate = CalculationEfficiency(gb55015_capacity.k1, float.Parse(gb55015_efficiency.k1), double.Parse(gb55015_IPLV_GB.k1), unit);
                        if (grate != 0)
                        {
                            gb55015title.k1 = "pass";
                        }
                        else
                        {
                            gb55015title.k1 = "Does not Comply. The unit fails to rate at GB conditions";
                            gb55015_capacity.k1 = "-";
                            gb55015_power.k1 = "-";
                            gb55015_efficiency.k1 = "-";
                            gb55015_loadP75.k1 = "-";
                            gb55015_loadP50.k1 = "-";
                            gb55015_loadP25.k1 = "-";
                            gb55015_IPLV_GB.k1 = "-";
                            gb55015_RLA.k1 = "-";
                        }
                    }
                }
                #endregion

                return "";
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        /// <summary>
        /// 生成rating输入  有GB区分
        /// </summary>
        /// <param name="projectInputDto"></param>
        /// <returns></returns>
        private RatingInputModel GenerateRatingInputData(ProjectInputDto projectInputDto, CapacityType capacityType)
        {
            try
            {
                var _proofdata = projectInputDto.proofData;
                RatingInputModel ratingInputModel = new RatingInputModel();
                string partLoadType = _proofdata.partloadType.paramValue;
                if (_proofdata.partloadDatalists.Count > 0)
                {
                    var inputdatalist = _proofdata.partloadDatalists[0].children;   ///计算出的冷量点数
                    if (inputdatalist.Count > 0)
                    {
                        ratingInputModel.Capacity = DataListToStr(inputdatalist[1]);
                        if ((_proofdata.evaflowEwt.paramValue == FluidCalType.flow_lwt &&  partLoadType != PointTypeConstants.m_cstrAHRI)
                            || partLoadType == PointTypeConstants.m_cstrGBConditon)   //  || partLoadType == PointTypeConstants.m_cstrUserDef
                        {
                            ratingInputModel.EvapFlow = DataListToStr(inputdatalist[2]);
                            ratingInputModel.CondFlow = DataListToStr(inputdatalist[4]);

                            ///拼接默认值
                            string eva_ewt = DataFormat.EffNumber(_proofdata.evaewt.paramValue);
                            string cond_lwt = DataFormat.EffNumber(_proofdata.condlwt.paramValue);
                            string eva_ewt_s = "";
                            string cond_lwt_s = "";
                            for (int i = 0; i < int.Parse(_proofdata.count.paramValue); i++)
                            {
                                eva_ewt_s += $"{eva_ewt},";
                                cond_lwt_s += $"{cond_lwt},";
                            }
                            ratingInputModel.EvapEWT = eva_ewt_s.TrimEnd(',');
                            ratingInputModel.CondLWT = cond_lwt_s.TrimEnd(',');
                        }
                        else
                        {
                            ratingInputModel.EvapEWT = DataListToStr(inputdatalist[2]);
                            ratingInputModel.CondLWT = DataListToStr(inputdatalist[4]);


                            string eva_flow = DataFormat.EffNumber(double.Parse(_proofdata.evaflow.paramValue));
                            string cond_flow = DataFormat.EffNumber(double.Parse(_proofdata.condflow.paramValue));
                            string eva_flow_s = "";
                            string cond_flow_s = "";
                            for (int i = 0; i < int.Parse(_proofdata.count.paramValue); i++)
                            {
                                eva_flow_s += $"{eva_flow},";
                                cond_flow_s += $"{cond_flow},";
                            }

                            ratingInputModel.EvapFlow = eva_flow_s.TrimEnd(',');
                            ratingInputModel.CondFlow = cond_flow_s.TrimEnd(',');
                        }


                        ratingInputModel.EvapLWT = DataListToStr(inputdatalist[3]);
                        ratingInputModel.CondEWT = DataListToStr(inputdatalist[5]);
                    }
                    else
                    {
                        return null;
                    }

                    ///删除多余结果数据 重新计算
                    if (_proofdata.partloadDatalists.Count > 1)
                    {
                        for (int i = 1; i < _proofdata.partloadDatalists.Count; i++)
                        {
                            _proofdata.partloadDatalists.RemoveAt(i);
                            i--;
                        }
                    }
                }
                else
                {
                    return null;
                }

                FillRatingInput(ratingInputModel, projectInputDto, capacityType);

                return ratingInputModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void FillRatingInput(RatingInputModel ratingInputModel, ProjectInputDto projectInputDto, CapacityType capacityType)
        {
            var _proofdata = projectInputDto.proofData;
            ratingInputModel.CondDesignPressure = float.Parse(_proofdata.conddesignPressure.paramValue);
            ratingInputModel.EvapDesignPressure = float.Parse(_proofdata.evadesignPressure.paramValue);

            ratingInputModel.EvapFluidType = _proofdata.evafluidType.paramValue;
            ratingInputModel.EvapFoul = float.Parse(_proofdata.evafoulingfactor.paramValue);
            ratingInputModel.EvapGlycolQty = float.Parse(_proofdata.evaglycolQty.paramValue);
            ratingInputModel.EvapVPFMinPct = float.Parse(_proofdata.evaporatorVpfMin.paramValue);


            ratingInputModel.CondFluidType = _proofdata.condfluidType.paramValue;
            ratingInputModel.CondFoul = float.Parse(_proofdata.condfoulingfactor.paramValue);
            ratingInputModel.CondGlycolQty = float.Parse(_proofdata.condglycolQty.paramValue);
            ratingInputModel.CondVPFMinPct = float.Parse(_proofdata.condenserVpfMin.paramValue);

            ratingInputModel.MeasurementSystem = projectInputDto.metricInch;
            ratingInputModel.StartType = _proofdata.starterType.paramValue;

            if (capacityType == CapacityType.NormalCapacity)
            {
                ratingInputModel.PointCount = int.Parse(_proofdata.count.paramValue);
                ratingInputModel.RegionalStandard = "None";
                ratingInputModel.RegionalStandard50189 = "None";
            }
            else if(capacityType == CapacityType.GB15977Capacity)
            {
                ratingInputModel.PointCount = 4;
                ratingInputModel.RegionalStandard = "GB";
                ratingInputModel.RegionalStandard50189 = "None";
            }
            else if (capacityType == CapacityType.GB55015Capacity)
            {
                ratingInputModel.PointCount = 4;
                ratingInputModel.RegionalStandard = "None";
                ratingInputModel.RegionalStandard50189 = "GB";

            }

            ///离心机拼接机型
            if (projectInputDto.modelType == (int)ModelType.Centrifuge)
            {
                var paramComparisonList = _paramComparisonRepository.FindAll().ToList();
                var power_code = paramComparisonList.Where(p => p.CategoryName == "inputPower" && p.FromParm == _proofdata.inputPwoer.paramValue).FirstOrDefault()?.ToParm;
                var chilertype_code = paramComparisonList.Where(p => p.CategoryName == nameof(_proofdata.chillerType) && p.FromParm == _proofdata.chillerType.paramValue).FirstOrDefault()?.ToParm;
                var impellerFamily_code = paramComparisonList.Where(p => p.CategoryName == nameof(_proofdata.impellerFamily) && p.FromParm == _proofdata.impellerFamily.paramValue).FirstOrDefault()?.ToParm;
                var impellerWidth_code = paramComparisonList.Where(p => p.CategoryName == nameof(_proofdata.impellerWidth) && p.FromParm == _proofdata.impellerWidth.paramValue).FirstOrDefault()?.ToParm;
                var gear_code = paramComparisonList.Where(p => p.CategoryName == nameof(_proofdata.gear) && p.FromParm == _proofdata.gear.paramValue).FirstOrDefault()?.ToParm;
                var moter_code = paramComparisonList.Where(p => p.CategoryName == nameof(_proofdata.moter) && p.FromParm == _proofdata.moter.paramValue).FirstOrDefault()?.ToParm;

                var evatype_code = paramComparisonList.Where(p => p.CategoryName == nameof(_proofdata.evaType) && p.FromParm == _proofdata.evaType.paramValue).FirstOrDefault()?.ToParm;
                var evadia_code = paramComparisonList.Where(p => p.CategoryName == "cylinderDia" && p.FromParm == _proofdata.evaDia.paramValue).FirstOrDefault()?.ToParm;
                var evalength_code = paramComparisonList.Where(p => p.CategoryName == "cylinderLength" && p.FromParm == _proofdata.evaLength.paramValue).FirstOrDefault()?.ToParm;
                var evatube_code = paramComparisonList.Where(p => p.CategoryName == nameof(_proofdata.evaTubeNum) && p.FromParm == _proofdata.evaTubeNum.paramValue).FirstOrDefault()?.ToParm;
                var evatube_type = _proofdata.evaTubeType.paramValue;
                var evapass = _proofdata.evapass.paramValue;

                var condtype = _proofdata.condType.paramValue;
                var conddia_code = paramComparisonList.Where(p => p.CategoryName == "cylinderDia" && p.FromParm == _proofdata.condDia.paramValue).FirstOrDefault()?.ToParm;
                var condlength_code = paramComparisonList.Where(p => p.CategoryName == "cylinderLength" && p.FromParm == _proofdata.condLength.paramValue).FirstOrDefault()?.ToParm;
                var condtube_code = paramComparisonList.Where(p => p.CategoryName == nameof(_proofdata.condTubeNum) && p.FromParm == _proofdata.condTubeNum.paramValue).FirstOrDefault()?.ToParm;
                var condtube_type = _proofdata.condTubeType.paramValue;
                var condpass = _proofdata.condpass.paramValue;

                ratingInputModel.ChillerModelCode = string.Format($"{chilertype_code}{impellerFamily_code}{impellerWidth_code}{gear_code}{moter_code}{power_code}/" +
                    $"{evatype_code}{evadia_code}{evalength_code}-{evatube_code}{evatube_type}-{evapass}/" +
                    $"{condtype}{conddia_code}{condlength_code}-{condtube_code}{condtube_type}-{condpass}");

            }
            else
            {
                ratingInputModel.ChillerModelCode = _proofdata.chillerCode.paramValue;
            }

            _proofdata.chillercode_input = ratingInputModel.ChillerModelCode;
            ratingInputModel.ChillerType = _proofdata.chillerCode.paramValue.Substring(0, 3);  //前三位 _proofdata.chillerCode.paramValue.Substring(0, 3)
            if (_proofdata.evaflowEwt.paramValue == FluidCalType.flow_lwt)
            {
                ratingInputModel.FluidCalType = 1;
            }
            else
            {
                ratingInputModel.FluidCalType = 2;
            }

            #region pointType
            string partLoadType = _proofdata.partloadType.paramValue;
            ratingInputModel.PointType = 0;
            if (partLoadType == PointTypeConstants.m_cstrAHRICR)
            {
                ratingInputModel.PointType = 0;
              
            }
            if (partLoadType == PointTypeConstants.m_cstrGBCR)
            {
                ratingInputModel.PointType = 1;
            }
            if (partLoadType == PointTypeConstants.m_cstrVPF)
            {
                ratingInputModel.PointType = 2;
            }
            if (partLoadType == PointTypeConstants.m_cstrUserDef)
            {
                ratingInputModel.PointType = 3;
            }
            if (partLoadType == PointTypeConstants.m_cstrAHRI)
            {
                ratingInputModel.PointType = 4;
                ///特殊
                ratingInputModel.FluidCalType = 2;
                ratingInputModel.EvapFoul = float.Parse(MeasurementSystem.FoulConvertor(0.0001, "IP", projectInputDto.metricInch).ToString());
                ratingInputModel.CondFoul = float.Parse(MeasurementSystem.FoulConvertor(0.00025, "IP", projectInputDto.metricInch).ToString());
            }
            if (partLoadType == PointTypeConstants.m_cstrGBConditon)
            {
                ratingInputModel.PointType = 5;
                ///特殊
                ratingInputModel.FluidCalType = 1;
                ratingInputModel.EvapFoul = float.Parse(MeasurementSystem.FoulConvertor(0.0180, "SI", projectInputDto.metricInch).ToString());
                ratingInputModel.CondFoul = float.Parse(MeasurementSystem.FoulConvertor(0.0440, "SI", projectInputDto.metricInch).ToString());
            }
            if (partLoadType == PointTypeConstants.m_cstrCCEWT)
            {
                ratingInputModel.PointType = 6;
            }
            if (partLoadType == PointTypeConstants.m_cstrVPFAHRICR)
            {
                ratingInputModel.PointType = 7;
            }
            #endregion

            ratingInputModel.Power = DataFormat.InputPowerFomat(_proofdata.inputPwoer.paramValue);//电制

            ratingInputModel.CustomSelectionType = "";
            ratingInputModel.HeatRecoverCapacity = "";
            ratingInputModel.HotByPass = "N";
            ratingInputModel.IsAHRICertification = false;
            ratingInputModel.IsCOPCurve = false;
            ratingInputModel.IsCustomSelection = false;
            ratingInputModel.IsSCFChillerSystem = false;
            ratingInputModel.LowLift = false;
            ratingInputModel.OilCooler = "ROC";
            ratingInputModel.OrderDetermination = false;
            ratingInputModel.Percentage = "";
            ratingInputModel.Qty = "1";
            ratingInputModel.ReportItems = "";
            ratingInputModel.RunningMode = "Cool model";
            ratingInputModel.StarterMountingType = "UM";
            ratingInputModel.StartTypeDown = "";
            ratingInputModel.StrPointType = "";
            ratingInputModel.Valve = "";
            ratingInputModel.VFDOption = "";
        }



        private string checkOutputList(ProjectInputDto projectInputDto,List<ChillerOutputList> chillerOutputList)
        {
            try
            {
                var proofData = projectInputDto.proofData;
                int PointCount = int.Parse(proofData.count.paramValue);
             
                if (chillerOutputList.Count > 0)
                {
                    ///异常处理
                    if (chillerOutputList[0].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat() == 0.0)  //异常信息获取
                    {
                        try
                        {
                            if (unit == "SI")
                            {
                                string[] message = new string[chillerOutputList[0].ErrorMessage.Split('\n').Count()];
                                string listmessage = "";
                                message = chillerOutputList[0].ErrorMessage.Split('\n');
                                double actual = 0;
                                double target = 0;
                                for (int i = 0; i < message.Count(); i++)
                                {
                                    if (message[i].Contains("Actual Capacity"))
                                    {
                                        string[] s1 = { "Capacity", "ton" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);

                                        actual = Convert.ToDouble(s2[1].Trim()) * 3.51685284206667f;

                                        target = Convert.ToDouble(proofData.capacity.paramValue);
                                        message[i] = "Actual Capacity " + actual.ToString("f1") + " kW is lower than the Requested Capacity " + target.ToString("f1") + " kW";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Evaporator Fluid Flow") && message[i].Contains("exceeds"))
                                    {
                                        string[] s1 = { "Flow", "ft/s", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = Convert.ToDouble(s2[1].Trim()) * 0.3048;
                                        target = Convert.ToDouble(s2[3].Trim()) * 0.3048;
                                        message[i] = "Evaporator Fluid Velocity " + actual.ToString("f1") + " m/s exceeds the limit " + target.ToString("f1") + " m/s";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Evaporator Fluid Flow") && message[i].Contains("lower"))
                                    {
                                        string[] s1 = { "Flow", "ft/s", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = Convert.ToDouble(s2[1].Trim()) * 0.3048;
                                        target = Convert.ToDouble(s2[3].Trim()) * 0.3048;
                                        message[i] = "Evaporator Fluid Velocity " + actual.ToString("f1") + " m/s is lower than the limit " + target.ToString("f1") + " m/s";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Condenser Fluid Velocity") && message[i].Contains("exceeds"))
                                    {
                                        string[] s1 = { "Velocity", "ft/s", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = Convert.ToDouble(s2[1].Trim()) * 0.3048;
                                        target = Convert.ToDouble(s2[3].Trim()) * 0.3048;
                                        message[i] = "Condenser Fluid Velocity " + actual.ToString("f1") + " m/s exceeds the limit " + target.ToString("f1") + " m/s";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Condenser Fluid Velocity") && message[i].Contains("lower"))
                                    {
                                        string[] s1 = { "Velocity", "ft/s", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = Convert.ToDouble(s2[1].Trim()) * 0.3048;
                                        target = Convert.ToDouble(s2[3].Trim()) * 0.3048;
                                        message[i] = "Condenser Fluid Velocity " + actual.ToString("f1") + " m/s is lower than the limit " + target.ToString("f1") + " m/s";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Evaporation Temperature") && message[i].Contains("exceeds"))
                                    {
                                        string[] s1 = { "Temperature", "°F", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim()) - 32) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim()) - 32) / 1.8;
                                        message[i] = "Evaporation Temperature " + actual.ToString("f1") + " ℃ exceeds the limit " + target.ToString("f1") + " ℃";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Evaporation Temperature") && message[i].Contains("low"))
                                    {
                                        string[] s1 = { "Temperature", "°F" };
                                        string[] s2 = message[i].Split(s1, 3, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim()) - 32) / 1.8;

                                        message[i] = "Evaporation Temperature " + actual.ToString("f1") + " ℃ is too low - must use EthlyGlycol";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Condensation Temperature") && message[i].Contains("exceeds"))
                                    {
                                        string[] s1 = { "Temperature", "°F", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim()) - 32) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim()) - 32) / 1.8;
                                        message[i] = "Condensation Temperature " + actual.ToString("f1") + " ℃ exceeds the limit " + target.ToString("f1") + " ℃";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("The maximum evaporator leaving water"))
                                    {
                                        string[] s1 = { "is", "°F" };
                                        string[] s2 = message[i].Split(s1, 3, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim()) - 32) / 1.8;

                                        message[i] = "The maximum evaporator leaving water temperature for the given conditions is " + actual.ToString("f1") + " ℃  ,please enter a lower value";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("The minimum evaporator leaving water"))
                                    {
                                        string[] s1 = { "is", "°F" };
                                        string[] s2 = message[i].Split(s1, 3, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim()) - 32) / 1.8;

                                        message[i] = "The minimum evaporator leaving water temperature for the given conditions is " + actual.ToString("f1") + " ℃  ,please enter a higher value";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Required motor power"))
                                    {
                                        string[] s1 = { "power", "HP", "maximum" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        target = Convert.ToDouble(s2[1].Trim()) * 0.7456999;
                                        actual = Convert.ToDouble(s2[3].Trim()) * 0.7456999;
                                        message[i] = "Required motor power " + target.ToString("f1") + " kW exceeds the motor maximum " + actual.ToString("f1") + " kW";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Refrigerant Charge"))
                                    {
                                        string[] s1 = { "Charge", "lb", "Capacity" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        target = Convert.ToDouble(s2[1].Trim()) * 0.4536;
                                        actual = Convert.ToDouble(s2[3].Trim()) * 0.4536;
                                        message[i] = "Refrigerant Charge " + target.ToString("f1") + " kg exceeds Pump Down Capacity " + actual.ToString("f1") + " kg";
                                        listmessage += message[i] + "\r\n";
                                    }

                                    else if (message[i].Contains("The condensing temperature") && message[i].Contains("Possible solutions"))
                                    {
                                        string[] s1 = { "temperature", "°F" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim()) - 32) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim()) - 32) / 1.8;
                                        message[i] = "The condensing temperature " + actual.ToString("f1") + " ℃ is higher than the maximum temperature " + target.ToString("f1") + " ℃ allowed for the unit. Possible solutions: Lower the ambient temperature or select a higher efficient model";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("The condensing temperature"))
                                    {
                                        string[] s1 = { "temperature", "°F" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim()) - 32) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim()) - 32) / 1.8;
                                        message[i] = "The condensing temperature " + actual.ToString("f1") + " ℃ is higher than the maximum temperature " + target.ToString("f1") + " ℃ allowed for the unit.";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("The suction temperature ") && message[i].Contains("the minimum temperature"))
                                    {
                                        string[] s1 = { "temperature", "°F" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim()) - 32) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim()) - 32) / 1.8;
                                        message[i] = "The suction temperature " + actual.ToString("f1") + " ℃ is lower than the minimum temperature " + target.ToString("f1") + " ℃ allowed by the compressor. Possible solutions: Raise the leaving evaporator water temperature or reduce the % Ethylene glycol content.";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("The suction temperature ") && message[i].Contains("the maximum temperature"))
                                    {
                                        string[] s1 = { "temperature", "°F" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim()) - 32) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim()) - 32) / 1.8;
                                        message[i] = "The suction temperature " + actual.ToString("f1") + " ℃ is higher than the maximumm temperature " + target.ToString("f1") + " ℃ allowed by the compressor. Possible solutions: Lower the leaving evaporator water temperature.";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Condenser Delta T") && message[i].Contains("exceeds the limit"))
                                    {
                                        string[] s1 = { "T", "°F", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim())) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim())) / 1.8;
                                        message[i] = "Condenser Delta T " + actual.ToString("f1") + " ℃ exceeds the limit " + target.ToString("f1") + " ℃ .";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Evaporator Delta T") && message[i].Contains("exceeds the limit"))
                                    {
                                        string[] s1 = { "T", "°F", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim())) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim())) / 1.8;
                                        message[i] = "Evaporator Delta T " + actual.ToString("f1") + " ℃ exceeds the limit " + target.ToString("f1") + " ℃ .";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Condenser Delta T") && message[i].Contains("exceeds the lower limit"))
                                    {
                                        string[] s1 = { "T", "°F", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim())) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim())) / 1.8;
                                        message[i] = "Condenser Delta T " + actual.ToString("f1") + " ℃ exceeds the lower limit " + target.ToString("f1") + " ℃ .";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else if (message[i].Contains("Evaporator Delta T") && message[i].Contains("exceeds the lower limit"))
                                    {
                                        string[] s1 = { "T", "°F", "limit" };
                                        string[] s2 = message[i].Split(s1, 5, StringSplitOptions.None);
                                        actual = (Convert.ToDouble(s2[1].Trim())) / 1.8;
                                        target = (Convert.ToDouble(s2[3].Trim())) / 1.8;
                                        message[i] = "Evaporator Delta T " + actual.ToString("f1") + " ℃ exceeds the lower limit " + target.ToString("f1") + " ℃ .";
                                        listmessage += message[i] + "\r\n";
                                    }
                                    else
                                    {
                                        listmessage += message[i] + "\r\n";
                                    }
                                }

                                return string.IsNullOrEmpty(listmessage) ? chillerOutputList[0].ErrorMessage : listmessage;
                            }
                            else
                                return chillerOutputList[0].ErrorMessage;
                        }
                        catch (Exception ex)
                        {
                            return chillerOutputList[0].ErrorMessage + ex.Message;
                        }
                    }
                    ///重新初始化clac
                    else
                    {
                        int ratingResultPointCount = chillerOutputList.Count;
                        string Capacity = proofData.capacity.paramValue;
                        string partloadtype = proofData.partloadType.paramValue;
                        string eva_flow_ewt = proofData.evaflowEwt.paramValue;
                        string eva_flow = proofData.evaflow.paramValue;
                        string cond_flow = proofData.condflow.paramValue;
                       

                        var loadPointData = proofData.partloadDatalists[0];
                        var _perloadlist = loadPointData.children[0];
                        var _capacitylist = loadPointData.children[1];
                        var _eva_flow_OR_ewt_list = loadPointData.children[2];
                        var _eva_lwt_list = loadPointData.children[3];
                        var _cond_flow_OR_lwt_list = loadPointData.children[4];
                        var _cond_ewt_list = loadPointData.children[5];
                    
                        for (int k = 1; k < 7; k++)
                        {
                            switch (k)
                            {
                                case 1:
                                    for (int i = 1; i <= PointCount; i++)
                                    {
                                        if (chillerOutputList[i - 1].Capacity > 0)
                                        {
                                            _perloadlist.GetType().GetProperty($"k{i}")?.SetValue(_perloadlist, DataFormat.DecNum((chillerOutputList[i - 1].Capacity.GetValueInBaseUnitsOfMeasureAsFloat() / chillerOutputList[0].Capacity.GetValueInBaseUnitsOfMeasureAsFloat() * 100), 1));
                                        }
                                        else
                                            _perloadlist.GetType().GetProperty($"k{i}")?.SetValue(_perloadlist, DataFormat.DecNum((chillerOutputList[i - 1].PercentageOfLoad * 100), 1));
                                    }
                                 
                                    break;
                                case 2:
                                    for (int i = 1; i <= PointCount; i++)
                                    {
                                        if (chillerOutputList[i - 1].Capacity > 0)
                                        {
                                            _capacitylist.GetType().GetProperty($"k{i}")?.SetValue(_capacitylist, DataFormat.EffNumber(MeasurementSystem.CapacityConvertor(chillerOutputList[i - 1].Capacity.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit), 4));
                                        }
                                        else
                                            _capacitylist.GetType().GetProperty($"k{i}")?.SetValue(_capacitylist, DataFormat.EffNumber(MeasurementSystem.CapacityConvertor(chillerOutputList[i - 1].PercentageOfLoad * double.Parse(Capacity), "SI", unit), 4));
                                    }
                                    break;
                                case 3:
                                  
                                    if ((partloadtype != PointTypeConstants.m_cstrAHRI && eva_flow_ewt==FluidCalType.flow_lwt) || partloadtype == PointTypeConstants.m_cstrUserDef || partloadtype == PointTypeConstants.m_cstrGBConditon)
                                    {
                                        for (int i = 1; i <= ratingResultPointCount; i++)
                                        {
                                            if (chillerOutputList[i - 1].Capacity > 0)
                                                _eva_flow_OR_ewt_list.GetType().GetProperty($"k{i}")?.SetValue(_eva_flow_OR_ewt_list, DataFormat.EffNumber(MeasurementSystem.FlowConvertor(chillerOutputList[i - 1].EvaporatorFlowrate.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit), 4));
                                            else
                                                _eva_flow_OR_ewt_list.GetType().GetProperty($"k{i}")?.SetValue(_eva_flow_OR_ewt_list, eva_flow);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 1; i <= ratingResultPointCount; i++)
                                        {
             
                                            if (chillerOutputList[i - 1].Capacity > 0)
                                                _eva_flow_OR_ewt_list.GetType().GetProperty($"k{i}")?.SetValue(_eva_flow_OR_ewt_list, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(chillerOutputList[i - 1].EvaporatorTin.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit), 4)));
                                            else
                                                _eva_flow_OR_ewt_list.GetType().GetProperty($"k{i}")?.SetValue(_eva_flow_OR_ewt_list, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(chillerOutputList[i - 1].EvaporatorTin.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit), 4)));
                                        }
                                    }
                                 
                                    break;
                                case 4:
                                    
                                    for (int i = 1; i <= ratingResultPointCount; i++)
                                    {
                                        if (chillerOutputList[i - 1].Capacity > 0)
                                            _eva_lwt_list.GetType().GetProperty($"k{i}")?.SetValue(_eva_lwt_list, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(chillerOutputList[i - 1].EvaporatorTout.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit), 4)));
                                        else
                                            _eva_lwt_list.GetType().GetProperty($"k{i}")?.SetValue(_eva_lwt_list, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(chillerOutputList[i - 1].EvaporatorTout.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit), 4)));
                                    }
                                  
                                    break;
                                case 5:
                                    

                                    if ((partloadtype != PointTypeConstants.m_cstrAHRI && eva_flow_ewt == FluidCalType.flow_lwt) || partloadtype == PointTypeConstants.m_cstrUserDef || partloadtype == PointTypeConstants.m_cstrGBConditon)
                                    {
                                       
                                        for (int i = 1; i <= ratingResultPointCount; i++)
                                        {
                                            if (chillerOutputList[i - 1].Capacity > 0)
                                                _cond_flow_OR_lwt_list.GetType().GetProperty($"k{i}")?.SetValue(_cond_flow_OR_lwt_list, DataFormat.EffNumber(MeasurementSystem.FlowConvertor(chillerOutputList[i - 1].CondenserFlowrate.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit), 4));
                                            else
                                                _cond_flow_OR_lwt_list.GetType().GetProperty($"k{i}")?.SetValue(_cond_flow_OR_lwt_list, cond_flow);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 1; i <= ratingResultPointCount; i++)
                                        {
                                            _cond_flow_OR_lwt_list.GetType().GetProperty($"k{i}")?.SetValue(_cond_flow_OR_lwt_list, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(chillerOutputList[i - 1].CondenserTout.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit), 4)));
  
                                        }
                                    }
                                
                                    break;
                                case 6:
                                 
                                
                                    for (int i = 1; i <= ratingResultPointCount; i++)
                                    {
                                        _cond_ewt_list.GetType().GetProperty($"k{i}")?.SetValue(_cond_ewt_list, DataFormat.EffNumberForTemp(DataFormat.EffNumber(MeasurementSystem.TempConvertor(chillerOutputList[i - 1].CondenserTin.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit), 4)));
                                      
                                    }
                            
                                    break;
                            }
                        }
                    }
                    //if (DataFormat.IsSubcoolingOver(chillerOutputList))
                    //{
                    //    return "The subcooling is too low, please increase the condenser delta T or decrease the condenser flow rate.";
                    //}
                }

                return "";

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
    
        }

        /// <summary>
        /// loadpoint 计算
        /// </summary>
        /// <param name="projectInputDto"></param>
        /// <returns></returns>
        public ProjectInputDto RatingMatrixInputData(ProjectInputDto projectInputDto)
        {
            try
            {
               var _proofdata = projectInputDto.proofData;
             
                List<string> cond_ewt_list = _proofdata?.loadPointTitle[0].children?.Where(p=>p.title.Contains("Condenser EWT")).FirstOrDefault()?.children?.Select(p=>p.title)?.ToList();
        
                int? point_count = _proofdata?.loadPointDatalists[0]?.children.Count;
                List<Task> task_list = new List<Task>();

                string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".csv";
                foreach (var loadpoint_group in _proofdata.loadPointDatalists)
                {
                  
                    int cond_ewt_start_index = 1;
                    StringBuilder cal_capacity = new StringBuilder();
                    loadpoint_group.children.ForEach(child => { cal_capacity.Append(child.capacity + ","); });

                    StringBuilder cal_eva_flow = new StringBuilder();
                    loadpoint_group.children.ForEach(child => { cal_eva_flow.Append(child.evapFlow + ","); });

                    StringBuilder cal_eva_lwt = new StringBuilder();
                    loadpoint_group.children.ForEach(child => { cal_eva_lwt.Append(child.evapLwt + ","); });

                    StringBuilder cal_cond_flow = new StringBuilder();
                    loadpoint_group.children.ForEach(child => { cal_cond_flow.Append(child.condFlow + ","); });

                    string eva_ewt = DataFormat.EffNumber(_proofdata.evaewt.paramValue);
                    string cond_lwt = DataFormat.EffNumber(_proofdata.condlwt.paramValue);
                    StringBuilder cal_eva_ewt = new StringBuilder();
                    StringBuilder cal_cond_lwt = new StringBuilder();
                    for (int i = 0; i < point_count; i++)
                    {
                        cal_eva_ewt.Append(eva_ewt + ",");
                        cal_cond_lwt.Append(cond_lwt + ",");
                    }

                    foreach (var cond_ewt_item in cond_ewt_list)
                    {
                        RatingInputModel ratingInputModel = new RatingInputModel();
                      
                        StringBuilder cal_cond_ewt = new StringBuilder();
                        for (int i = 0; i < point_count; i++)
                        {
                            cal_cond_ewt.Append(cond_ewt_item + ",");
                        }

                        ratingInputModel.Capacity = cal_capacity.ToString().TrimEnd(',');

                        ratingInputModel.EvapFlow = cal_eva_flow.ToString().TrimEnd(',');
                        ratingInputModel.EvapLWT = cal_eva_lwt.ToString().TrimEnd(',');


                        ratingInputModel.CondFlow = cal_cond_flow.ToString().TrimEnd(',');
                        ratingInputModel.CondEWT = cal_cond_ewt.ToString().TrimEnd(',');

                        //拼接默认值？
                        ratingInputModel.EvapEWT = cal_eva_ewt.ToString().TrimEnd(',');
                        ratingInputModel.CondLWT = cal_cond_lwt.ToString().TrimEnd(',');


                        FillRatingInput(ratingInputModel,projectInputDto, CapacityType.NormalCapacity);
                        ratingInputModel.PointCount = (int)point_count;
                        #region 界面值
                        //ratingInputModel.CondDesignPressure = float.Parse(_proofdata.conddesignPressure.paramValue);
                        //ratingInputModel.EvapDesignPressure = float.Parse(_proofdata.evadesignPressure.paramValue);

                        //ratingInputModel.EvapFluidType = _proofdata.evafluidType.paramValue;
                        //ratingInputModel.EvapFoul = float.Parse(_proofdata.evafoulingfactor.paramValue);
                        //ratingInputModel.EvapGlycolQty = float.Parse(_proofdata.evaglycolQty.paramValue);
                        //ratingInputModel.EvapVPFMinPct = float.Parse(_proofdata.evaporatorVpfMin.paramValue);

                        //ratingInputModel.CondFluidType = _proofdata.condfluidType.paramValue;
                        //ratingInputModel.CondFoul = float.Parse(_proofdata.condfoulingfactor.paramValue);
                        //ratingInputModel.CondGlycolQty = float.Parse(_proofdata.condglycolQty.paramValue);
                        //ratingInputModel.CondVPFMinPct = float.Parse(_proofdata.condenserVpfMin.paramValue);

                        //ratingInputModel.MeasurementSystem = projectInputDto.metricInch;
                        //ratingInputModel.StartType = _proofdata.starterType.paramValue;

                        //if (capacityType == CapacityType.NormalCapacity)
                        //{
                        //    ratingInputModel.PointCount = (int)point_count;
                        //    ratingInputModel.RegionalStandard = "None";
                        //    ratingInputModel.RegionalStandard50189 = "None";
                        //}
                        //else
                        //{
                        //    ratingInputModel.PointCount = 4;
                        //    ratingInputModel.RegionalStandard = _proofdata.gb19577.paramValue;
                        //    ratingInputModel.RegionalStandard50189 = _proofdata.gb55015.paramValue;
                        //}


                        //ratingInputModel.ChillerModelCode = _proofdata.chillerCode.paramValue;   ///是否区分 模块机
                        //ratingInputModel.ChillerType = _proofdata.chillerCode.paramValue.Substring(0, 3);  //前三位 _proofdata.chillerCode.paramValue.Substring(0, 3)
                        //if (_proofdata.evaflowEwt.paramValue == FluidCalType.flow_lwt)
                        //{
                        //    ratingInputModel.FluidCalType = 1;
                        //}
                        //else
                        //{
                        //    ratingInputModel.FluidCalType = 2;
                        //}

                        //#region pointType
                        //string partLoadType = _proofdata.partloadType.paramValue;
                        //ratingInputModel.PointType = 0;
                        //if (partLoadType == PointTypeConstants.m_cstrAHRICR)
                        //{
                        //    ratingInputModel.PointType = 0;
                        //}
                        //if (partLoadType == PointTypeConstants.m_cstrGBCR)
                        //{
                        //    ratingInputModel.PointType = 1;
                        //}
                        //if (partLoadType == PointTypeConstants.m_cstrVPF)
                        //{
                        //    ratingInputModel.PointType = 2;
                        //}
                        //if (partLoadType == PointTypeConstants.m_cstrUserDef)
                        //{
                        //    ratingInputModel.PointType = 3;
                        //}
                        //if (partLoadType == PointTypeConstants.m_cstrAHRI)
                        //{
                        //    ratingInputModel.PointType = 4;
                        //}
                        //if (partLoadType == PointTypeConstants.m_cstrGBConditon)
                        //{
                        //    ratingInputModel.PointType = 5;
                        //}
                        //if (partLoadType == PointTypeConstants.m_cstrCCEWT)
                        //{
                        //    ratingInputModel.PointType = 6;
                        //}
                        //if (partLoadType == PointTypeConstants.m_cstrVPFAHRICR)
                        //{
                        //    ratingInputModel.PointType = 7;
                        //} 
                        //#endregion

                        //ratingInputModel.Power = DataFormat.InputPowerFomat(_proofdata.inputPwoer.paramValue);


                        //ratingInputModel.CustomSelectionType = "";
                        //ratingInputModel.HeatRecoverCapacity = "";
                        //ratingInputModel.HotByPass = "N";
                        //ratingInputModel.IsAHRICertification = false;
                        //ratingInputModel.IsCOPCurve = false;
                        //ratingInputModel.IsCustomSelection = false;
                        //ratingInputModel.IsSCFChillerSystem = false;
                        //ratingInputModel.LowLift = false;
                        //ratingInputModel.OilCooler = "ROC";
                        //ratingInputModel.OrderDetermination = false;
                        //ratingInputModel.Percentage = "";
                        //ratingInputModel.Qty = "1";
                        //ratingInputModel.ReportItems = "";
                        //ratingInputModel.RunningMode = "Cool model";
                        //ratingInputModel.StarterMountingType = "UM";
                        //ratingInputModel.StartTypeDown = "";
                        //ratingInputModel.StrPointType = "";
                        //ratingInputModel.Valve = "";
                        //ratingInputModel.VFDOption = "";
                        #endregion

                        ///计算一组数据
                        RatingEngineNew ratingEngineNew = new RatingEngineNew();
                       
                        var normalRatingRe = ratingEngineNew.RunRating(ratingInputModel, true, false); //此处后期可能出现模块机selection模式要修改

                        ///获取一列cop
                        for (int i = 0; i < loadpoint_group.children.Count; i++)
                        {
                            if (normalRatingRe.Count > i)
                            {
                                var partload = loadpoint_group.children[i];
                                var prop = partload?.GetType().GetProperty($"k{cond_ewt_start_index}");
                                if (prop != null)
                                {
                                    prop.SetValue(partload, DataFormat.EffNumber(MeasurementSystem.EfficiencyConvertor(normalRatingRe[i].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat(), "SI", unit)));
                                }
                            }
                            else
                            {
                                break;
                            }

                        }
                      
                        cond_ewt_start_index++;
                    }

                    ///保存测试数据
                    using (StreamWriter sw = new StreamWriter(fileName, true))
                    {
                        Console.WriteLine($"{cal_eva_lwt.ToString().Split(',')[0]}*************");
                        sw.WriteLine($"{cal_eva_lwt.ToString().Split(',')[0]}*************");
                        for (int i = 0; i < loadpoint_group.children.Count; i++)
                        {
                            StringBuilder resb = new StringBuilder();
                            var partload = loadpoint_group.children[i];
                            resb.Append(partload.capacity + ",");
                            resb.Append(partload.percentLoad + ",");
                            resb.Append(partload.evapFlow + ",");
                            resb.Append(partload.evapLwt + ",");
                            resb.Append(partload.condFlow + ",");
                            for (int c = 1; c < cond_ewt_start_index; c++)
                            {
                                var prop = partload?.GetType().GetProperty($"k{c}");
                                resb.Append(prop.GetValue(partload) + ",");
                            }
                            sw.WriteLine(resb.ToString());
                            Console.WriteLine(resb.ToString());
                        }
                    }
                  

                }
             
                return projectInputDto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       
        /// <summary>
        /// 根据GB 生成输出参数
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="resultDataTitle"></param>
        /// <returns></returns>
        private List<PartloadDatalist_group> GenerateEmptyDataList(string unit, ResultDataTitle resultDataTitle,bool gb19577,bool gb55015)
        {
            List<PartloadDatalist_group> partloadDatalist_Groups = new List<PartloadDatalist_group>();
            List<string> resultData = new List<string>() { resultDataTitle.acapacity, resultDataTitle.inputpower, resultDataTitle.COP, resultDataTitle.PLV_NAME, resultDataTitle.RLA };
          
            if (gb19577)
            {
                resultData.Add(resultDataTitle.gb15977title);
                resultData.Add(resultDataTitle.acapacity);
                resultData.Add(resultDataTitle.inputpower);
                resultData.Add(resultDataTitle.COP);
                resultData.Add(resultDataTitle.load_P75);
                resultData.Add(resultDataTitle.load_P50);
                resultData.Add(resultDataTitle.load_P25);
                resultData.Add(ResultDataTitle.IPLVGB);
                resultData.Add(resultDataTitle.RLA);
            }
            if (gb55015)
            {
                resultData.Add(resultDataTitle.gb55015title);
                resultData.Add(resultDataTitle.acapacity);
                resultData.Add(resultDataTitle.inputpower);
                resultData.Add(resultDataTitle.COP);
                resultData.Add(resultDataTitle.load_P75);
                resultData.Add(resultDataTitle.load_P50);
                resultData.Add(resultDataTitle.load_P25);
                resultData.Add(ResultDataTitle.IPLVGB);
                resultData.Add(resultDataTitle.RLA);
            }

            var g2 = GenerateOneEmptyGroup(resultDataTitle, "2", resultDataTitle.result, resultData.ToArray());

            var g3 = GenerateOneEmptyGroup(resultDataTitle, "3", resultDataTitle.evadata, resultDataTitle.evaewt, resultDataTitle.evalwt, resultDataTitle.evaflow, resultDataTitle.predrop);
            var g4 = GenerateOneEmptyGroup(resultDataTitle, "4", resultDataTitle.conddata, resultDataTitle.condewt, resultDataTitle.condlwt, resultDataTitle.condflow, resultDataTitle.predrop);
            var g5 = GenerateOneEmptyGroup(resultDataTitle, "5", resultDataTitle.eledata, resultDataTitle.MOCP, resultDataTitle.powerfactor, resultDataTitle.outputAmps);
            var g6 = GenerateOneEmptyGroup(resultDataTitle, "6", resultDataTitle.servicedata, resultDataTitle.evatemp, resultDataTitle.evapressure, resultDataTitle.evawater, 
                                                                 resultDataTitle.condtemp, resultDataTitle.condpressure, resultDataTitle.condwater, resultDataTitle.superheat, resultDataTitle.subcooling);
            var g7 = GenerateOneEmptyGroup(resultDataTitle, "7", resultDataTitle.mes,"mes");

            partloadDatalist_Groups.Add(g2);
            partloadDatalist_Groups.Add(g3);
            partloadDatalist_Groups.Add(g4);
            partloadDatalist_Groups.Add(g5);
            partloadDatalist_Groups.Add(g6);
            partloadDatalist_Groups.Add(g7);

            return partloadDatalist_Groups;
        }

        private PartloadDatalist_group GenerateOneEmptyGroup(ResultDataTitle resultDataTitle, string key, string name, params string[] title)
        {
            PartloadDatalist_group result_g = new PartloadDatalist_group();
            result_g.key = key;
            result_g.name = name;
            int i = 1;
            foreach (string s in title)
            {
                PartloadDatalist partloadDatalist = new PartloadDatalist { 
                    name = s,
                    key = $"{key}.{i}"
                };
                i++;
                result_g.children.Add(partloadDatalist);
            }

            return result_g;
        }

        /// <summary>
        /// 生成loadpoint 入参数据
        /// </summary>
        /// <param name="projectInputDto"></param>
        /// <returns></returns>
        public ProjectInputDto GenerateLoadPointDto(ProjectInputDto projectInputDto)
        {
            try
            {
                unit = projectInputDto.metricInch;
                var proofdata = projectInputDto.proofData;


                var max_percent = proofdata.maxPercentLoad.paramValue.ObjectToDouble();
                var min_percent = proofdata.minPercentLoad.paramValue.ObjectToDouble();
                var step_percent = proofdata.stepPercentLoad.paramValue.ObjectToDouble();

                int pointCount = 0;
                if ((max_percent - min_percent) % step_percent == 0)
                {
                    pointCount = (int)((max_percent - min_percent) / step_percent) + 1;
                }
                else
                {
                    pointCount = (int)Math.Floor((max_percent - min_percent) / step_percent) + 1;
                }

                var max_cewt = proofdata.maxCewt.paramValue.ObjectToDouble();
                var min_cewt = proofdata.minCewt.paramValue.ObjectToDouble();
                var step_cewt = proofdata.stepCewt.paramValue.ObjectToDouble();

                var max_elwt = proofdata.maxElwt.paramValue.ObjectToDouble();
                var min_elwt = proofdata.minElwt.paramValue.ObjectToDouble();
                var step_elwt = proofdata.stepElwt.paramValue.ObjectToDouble();

                var capacity = proofdata.capacity.paramValue.ObjectToDouble();

                string partLoadType = proofdata.partloadType.paramValue;
                string eva_flow_ewt = proofdata.evaflowEwt.paramValue;
                string cond_flow_ewt = proofdata.condflowEwt.paramValue;
                string eva_flow = proofdata.evaflow.paramValue;
                string eva_lwt = proofdata.evalwt.paramValue.ToString();
                string eva_ewt = proofdata.evaewt.paramValue.ToString();
                string cond_flow = proofdata.condflow.paramValue;
                string cond_lwt = proofdata.condlwt.paramValue.ToString();
                string cond_ewt = proofdata.condewt.paramValue.ToString();
                string In_VPF_Evaporator_MinPct = proofdata.evaporatorVpfMin.paramValue;
                string In_VPF_Condenser_MinPct = proofdata.condenserVpfMin.paramValue;
                string In_CalculationType = proofdata.calculationType.paramValue;

                proofdata.loadPointTitle = GenerateLoadPointTile(max_cewt, min_cewt, step_cewt);
                proofdata.loadPointDatalists = new List<PartloadDatalist_group>();  //初始化 清空数据

                string temp_unit = unit == "SI" ? "℃" : "℉";
                for (double i = max_elwt; i >= min_elwt; i = i - step_elwt)
                {
                    PartloadDatalist_group partloadDatalist_Group = new PartloadDatalist_group();
                    partloadDatalist_Group.capacity = string.Format($"Load Point Type: Evap LWT:{i} {temp_unit}");
                    partloadDatalist_Group.key = i.ToString();
                    proofdata.loadPointDatalists.Add(partloadDatalist_Group);


                    for (int k = 1; k <= pointCount; k++)
                    {
                        PartloadDatalist partload = new PartloadDatalist();     //一行数据
                        partload.key = string.Format($"{i.ToString()}-{k.ToString()}");
                        partload.capacity = DataFormat.EffNumber((Convert.ToDouble(capacity.ToString().Trim()) / pointCount * (pointCount - k + 1)));  //冷量
                        partload.percentLoad = DataFormat.EffNumberForTemp((Convert.ToInt32(100.0 / pointCount * (pointCount - k + 1)).ToString()));      //percent load
                        if (partLoadType == PointTypeConstants.m_cstrVPF || partLoadType == PointTypeConstants.m_cstrVPFAHRICR)
                        {
                            if (In_CalculationType.Equals("Standard Performance"))
                            {
                                partload.evapFlow = getEvapFlow(k, pointCount, float.Parse(eva_flow), capacity.ToString(), In_VPF_Evaporator_MinPct, partLoadType);
                                partload.condFlow = getCondFlow(k, pointCount, float.Parse(cond_flow), capacity.ToString(), In_VPF_Condenser_MinPct, partLoadType);
                            }
                            else
                            {
                                partload.evapFlow = getEvapFlow(k / 2 + 1, pointCount, float.Parse(eva_flow), capacity.ToString(), In_VPF_Evaporator_MinPct, partLoadType);
                                partload.condFlow = getCondFlow(k / 2 + 1, pointCount, float.Parse(cond_flow), capacity.ToString(), In_VPF_Condenser_MinPct, partLoadType);
                            }
                        }
                        else
                        {
                            partload.evapFlow = eva_flow;
                            partload.condFlow = cond_flow;
                        }

                        partload.evapLwt = DataFormat.EffNumberForTemp(i.ToString()); //getEvapLWT(k, pointCount, (float)i, partLoadType);                          //eva lwt

                        partloadDatalist_Group.children.Add(partload);
                    }
                }

                return projectInputDto;
            }
            catch (Exception ex)
            {

                throw ex;
            }
          
        }

        public List<LoadPointTitle> GenerateLoadPointTile(double max_cewt,double min_cewt,double step_cewt)
        {
            ResultDataTitle resultDataTitle = new ResultDataTitle("");
            if (unit != "SI")
            {
                TitleUnitConvert(resultDataTitle);
            }
            //StringBuilder title = new StringBuilder();
            //title.Append(resultDataTitle.capacity + ",");
            //title.Append(resultDataTitle.percofFullLoad + ",");
            //title.Append(resultDataTitle.evaflow + ",");
            //title.Append(resultDataTitle.evalwt + ",");
            //title.Append(resultDataTitle.condflow + ",");
            //title.Append(resultDataTitle.condewt + ",");


            List<LoadPointTitle> loadPointTitleList = new List<LoadPointTitle>();
             LoadPointTitle loadPointTitle = new LoadPointTitle();
            loadPointTitle.title = "LoadPoint Output";
            loadPointTitle.children = new List<LoadPointTitleChild>();

            LoadPointTitleChild capacityTitle = new LoadPointTitleChild();
            capacityTitle.title = resultDataTitle.capacity;
            capacityTitle.dataIndex = "capacity";
            capacityTitle.key = "capacity";
            capacityTitle.width = 250;

            LoadPointTitleChild percentLoadTtile = new LoadPointTitleChild();
            percentLoadTtile.title = resultDataTitle.percofFullLoad;
            percentLoadTtile.dataIndex = "percentLoad";
            percentLoadTtile.key = "percentLoad";

            LoadPointTitleChild evaflowTitle = new LoadPointTitleChild();
            evaflowTitle.title = resultDataTitle.evaflow;
            evaflowTitle.dataIndex = "evapFlow";
            evaflowTitle.key = "evapFlow";

            LoadPointTitleChild evalwtTitle = new LoadPointTitleChild();
            evalwtTitle.title = resultDataTitle.evalwt;
            evalwtTitle.dataIndex = "evapLwt";
            evalwtTitle.key = "evapLwt";

            LoadPointTitleChild condflowTitle = new LoadPointTitleChild();
            condflowTitle.title = resultDataTitle.condflow;
            condflowTitle.dataIndex = "condFlow";
            condflowTitle.key = "condFlow";

            LoadPointTitleChild condewtTitle = new LoadPointTitleChild();
            condewtTitle.title = resultDataTitle.condewt;
            condewtTitle.children = new List<LoadPointTitleChild>();
            int INDEX = 1;
            for (double i = max_cewt; i >= min_cewt; i = i - step_cewt)
            {
                LoadPointTitleChild condewtTitleChild = new LoadPointTitleChild();
                condewtTitleChild.title = i.ToString("F2");
                condewtTitleChild.dataIndex = $"k{INDEX}";
                condewtTitleChild.key = $"k{INDEX}";
                condewtTitle.children.Add(condewtTitleChild);
                INDEX++;
            }


            loadPointTitle.children.Add(capacityTitle);
            loadPointTitle.children.Add(percentLoadTtile);
            loadPointTitle.children.Add(evaflowTitle);
            loadPointTitle.children.Add(evalwtTitle);
            loadPointTitle.children.Add(condflowTitle);
            loadPointTitle.children.Add(condewtTitle);
            loadPointTitleList.Add(loadPointTitle);
            return loadPointTitleList;
        }


        private void SaveInputRatingInputModel(RatingInputModel ratingInputModel)
        {
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                string v = JsonSerializer.Serialize(ratingInputModel);

                sw.WriteLine(v);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partLoadType"></param>
        /// <param name="eva_flow_ewt"></param>
        /// <param name="partload"></param>
        /// <param name="PointCount"></param>
        /// <param name="eva_flow"></param>
        /// <param name="Capacity"></param>
        private string GetEvapFlowOrEwt(string partLoadType, string eva_flow_ewt, int Pt_index, int PointCount, string eva_flow, string eva_ewt, string Capacity, string In_VPF_Evaporator_MinPct)
        {
            double minflow = 0;
            if (partLoadType == PointTypeConstants.m_cstrVPF || partLoadType == PointTypeConstants.m_cstrVPFAHRICR)
            {
                if (minflow > getEvapFlow(Pt_index, PointCount, float.Parse(eva_flow), Capacity, In_VPF_Evaporator_MinPct, partLoadType).ToPares())
                {
                    return minflow.ToString();
                }
                else
                {
                    return getEvapFlow(Pt_index, PointCount, float.Parse(eva_flow), Capacity, In_VPF_Evaporator_MinPct, partLoadType);
                }
            }
            else
            {
                return getEvapFlow(Pt_index, PointCount, float.Parse(eva_flow), Capacity, In_VPF_Evaporator_MinPct, partLoadType);
            }

        }


        private string GetCondFlowOrLwt(string partLoadType, string eva_flow_ewt, int Pt_index, int PointCount, string cond_flow, string cond_lwt, string Capacity, string In_VPF_Condenser_MinPct, string calTpye)
        {
          
            double minflow2 = 0;
            if (partLoadType == PointTypeConstants.m_cstrVPF || partLoadType == PointTypeConstants.m_cstrVPFAHRICR)
            {
                if (minflow2 > getCondFlow(Pt_index, PointCount, float.Parse(cond_flow), Capacity, In_VPF_Condenser_MinPct, partLoadType).ToPares())
                {
                    return minflow2.ToString();
                }
                else
                {

                    return getCondFlow(Pt_index, PointCount, float.Parse(cond_flow), Capacity, In_VPF_Condenser_MinPct, partLoadType);
                }
            }
            else
            {

                return getCondFlow(Pt_index, PointCount, float.Parse(cond_flow), Capacity, In_VPF_Condenser_MinPct, partLoadType);
            }

        }

        public string DataListToStr(PartloadDatalist datalist)
        {
            string datastr = "";
            for (int i = 1; i <= 10; i++)
            {
                var prop = datalist.GetType().GetProperty($"k{i}");
                if (prop != null)
                {
                    var pv = prop.GetValue(datalist)?.ToString();
                    if (!string.IsNullOrEmpty(pv))
                    {
                        datastr += pv + ",";
                    }
                }
            }
            datastr = datastr.TrimEnd(',');

            return datastr;
        }

        /// <summary>
        /// 输入参数转换
        /// </summary>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <param name="projectInputDto"></param>
        /// <returns></returns>
        public ProjectInputDto UnitConvert(string fromUnit, string toUnit, ProjectInputDto projectInputDto)
        {
           
            var inputparams = projectInputDto.proofData;
            if (fromUnit != toUnit)
            {
                ///冷量
                inputparams.capacity.paramUnit = MeasurementSystem.UnitTransDic[inputparams.capacity.paramUnit];
                inputparams.capacity.paramValue =DataFormat.EffNumber(MeasurementSystem.CapacityConvertor(inputparams.capacity.paramValue.ToPares(),fromUnit,toUnit));
                ///流量
                inputparams.evaflow.paramUnit = MeasurementSystem.UnitTransDic[inputparams.evaflow.paramUnit];
                inputparams.evaflow.paramValue = DataFormat.EffNumber(MeasurementSystem.FlowConvertor(inputparams.evaflow.paramValue.ToPares(), fromUnit, toUnit));
                ///进水温度
                inputparams.evaewt.paramUnit = MeasurementSystem.UnitTransDic[inputparams.evaewt.paramUnit];
                inputparams.evaewt.paramValue =MeasurementSystem.TempConvertor(inputparams.evaewt.paramValue, fromUnit, toUnit);
                ///出水温度
                inputparams.evalwt.paramUnit = MeasurementSystem.UnitTransDic[inputparams.evalwt.paramUnit];
                inputparams.evalwt.paramValue = MeasurementSystem.TempConvertor(inputparams.evalwt.paramValue, fromUnit, toUnit);
                ///污垢系数
                inputparams.evafoulingfactor.paramUnit = MeasurementSystem.UnitTransDic[inputparams.evafoulingfactor.paramUnit];
                inputparams.evafoulingfactor.paramValue = DataFormat.EffNumber(MeasurementSystem.FoulConvertor(inputparams.evafoulingfactor.paramValue.ToPares(), fromUnit, toUnit),3);
                ///水侧承压
                inputparams.evadesignPressure.paramUnit = MeasurementSystem.UnitTransDic[inputparams.evadesignPressure.paramUnit];
                inputparams.evadesignPressure.paramValue = MeasurementSystem.DesignPressureConvertor(inputparams.evadesignPressure.paramValue, fromUnit, toUnit);
                inputparams.evadesignPressure.paramComboxValue = new List<DropDownListData> { new DropDownListData { label = "150",value = "150",selected = true},new DropDownListData { label = "232", value = "232", selected = false } };


                ///流量
                inputparams.condflow.paramUnit = MeasurementSystem.UnitTransDic[inputparams.condflow.paramUnit];
                inputparams.condflow.paramValue = DataFormat.EffNumber(MeasurementSystem.FlowConvertor(inputparams.condflow.paramValue.ToPares(), fromUnit, toUnit));
                ///进水温度
                inputparams.condewt.paramUnit = MeasurementSystem.UnitTransDic[inputparams.condewt.paramUnit];
                inputparams.condewt.paramValue = MeasurementSystem.TempConvertor(inputparams.condewt.paramValue, fromUnit, toUnit);
                ///出水温度
                inputparams.condlwt.paramUnit = MeasurementSystem.UnitTransDic[inputparams.condlwt.paramUnit];
                inputparams.condlwt.paramValue = MeasurementSystem.TempConvertor(inputparams.condlwt.paramValue, fromUnit, toUnit);
                ///污垢系数
                inputparams.condfoulingfactor.paramUnit = MeasurementSystem.UnitTransDic[inputparams.condfoulingfactor.paramUnit];
                inputparams.condfoulingfactor.paramValue = DataFormat.EffNumber(MeasurementSystem.FoulConvertor(inputparams.condfoulingfactor.paramValue.ToPares(), fromUnit, toUnit),3);
                ///水侧承压
                inputparams.conddesignPressure.paramUnit = MeasurementSystem.UnitTransDic[inputparams.conddesignPressure.paramUnit];
                inputparams.conddesignPressure.paramValue = MeasurementSystem.DesignPressureConvertor(inputparams.conddesignPressure.paramValue, fromUnit, toUnit);
                inputparams.conddesignPressure.paramComboxValue = new List<DropDownListData> { new DropDownListData { label = "150", value = "150", selected = true }, new DropDownListData { label = "232", value = "232", selected = false } };


                projectInputDto.metricInch = toUnit;

            }
           
            return projectInputDto;
        }

        public ResultDataTitle TitleUnitConvert(ResultDataTitle resultDataTitle)
        {
            Regex regex = new Regex(@"\((.*?)\)");

            var props = resultDataTitle.GetType().GetProperties();
            foreach (var prop in props)
            {
                string old_value = prop.GetValue(resultDataTitle)?.ToString();
                string unit = regex.Match(old_value).Groups[1].Value;
                if (!string.IsNullOrEmpty(unit) && MeasurementSystem.UnitTransDic.ContainsKey(unit))
                {
                    string new_value = old_value.Replace(unit, MeasurementSystem.UnitTransDic[unit]);
                    prop.SetValue(resultDataTitle, Convert.ChangeType(new_value, prop.PropertyType));
                }
            }
         
            return resultDataTitle;
        }

        #region 工况初始化方法
        public string getEvapEWT(int i, int pointCount, float evapEWTFullLoad, string pointType)
        {
            string evapEWT = DataFormat.EffNumberForTemp(evapEWTFullLoad.ToString());
            double percentageOfLoad = Math.Round(1.0 / pointCount * (pointCount - i + 1), 2);
            if (pointType == PointTypeConstants.m_cstrAHRI)//工况下拉框引用时改索引为常量字符串 
            {
                if (percentageOfLoad == 1.0)
                {
                    evapEWT = MeasurementSystem.TempConvertor(54, "IP", unit).ToString("F2");
                }
                else
                {
                    evapEWT = "";
                }
            }
            if (pointType == PointTypeConstants.m_cstrGBConditon)//工况下拉框引用时改索引为常量字符串 
            {
                evapEWT = MeasurementSystem.TempConvertor(7, "SI", unit).ToString("F2");
            }
            return evapEWT;
        }


        public string getEvapLWT(int i, int pointCount, float evapLWTFullLoad, string pointType)
        {
            string evapLWT = evapLWTFullLoad.ToString("F2");
            double percentageOfLoad = Math.Round(1.0 / pointCount * (pointCount - i + 1), 2);
            if (pointType == PointTypeConstants.m_cstrAHRI)//工况下拉框引用时改索引为常量字符串 
            {
                evapLWT = MeasurementSystem.TempConvertor(44, "IP", unit).ToString("F2");
            }
            if (pointType == PointTypeConstants.m_cstrGBConditon)//工况下拉框引用时改索引为常量字符串 
            {
                evapLWT = MeasurementSystem.TempConvertor(7, "SI", unit).ToString("F2");
            }
            return evapLWT;
        }

        //工况下拉框引用时改索引为常量字符串 
        public string getCondEWT(int i, int pointCount, float condEWTFullLoad,string In_CondEWT, string pointType)
        {
            string condEWT = condEWTFullLoad.ToString("F2");
            double percentageOfLoad = Math.Round(1.0 / pointCount * (pointCount - i + 1), 2);
            //工况下拉框引用时改索引为常量字符串 
            if (pointType == PointTypeConstants.m_cstrAHRICR || pointType == PointTypeConstants.m_cstrUserDef || pointType == PointTypeConstants.m_cstrVPFAHRICR)//增加VPF Condenser relief工况
            {
                if (percentageOfLoad == 1.0)
                {
                    condEWT = condEWT;
                }
                else if (percentageOfLoad < 1 && percentageOfLoad > 0.5)
                {
                    condEWT = MeasurementSystem.TempConvertor((MeasurementSystem.TempConvertor(condEWTFullLoad, unit, "SI") - 18.3333333) * (percentageOfLoad * 2 - 1) + 18.3333333, "SI", unit).ToString("F2");
                }
                else if (percentageOfLoad <= 0.5)
                {
                    condEWT = MeasurementSystem.TempConvertor((65 - 32) / 1.8, "SI", unit).ToString("F2");
                }
            }
            else if (pointType == PointTypeConstants.m_cstrGBCR)//工况下拉框引用时改索引为常量字符串
            {
                if (percentageOfLoad == 1.0)
                {
                    condEWT = condEWT;

                }
                else if (percentageOfLoad < 1 && percentageOfLoad > 0.25)
                {
                    condEWT = DataFormat.EffNumberForTemp(MeasurementSystem.TempConvertor((MeasurementSystem.TempConvertor(condEWTFullLoad, unit, "SI") - 15.5) * percentageOfLoad + 15.5, "SI", unit).ToString());
                }
                else if (percentageOfLoad <= 0.5)
                {
                    condEWT = DataFormat.EffNumberForTemp(MeasurementSystem.TempConvertor(19.0, "SI", unit).ToString());
                }
            }
            else if (pointType == PointTypeConstants.m_cstrAHRI)//工况下拉框引用时改索引为常量字符串 
            {
                if (percentageOfLoad == 1.0)
                {
                    condEWT = DataFormat.EffNumberForTemp(MeasurementSystem.TempConvertor(85, "IP", unit).ToString());
                }
                else if (percentageOfLoad == 0.75)
                {
                    condEWT = DataFormat.EffNumberForTemp(MeasurementSystem.TempConvertor(75, "IP", unit).ToString());
                }
                else if (percentageOfLoad <= 0.5)
                {
                    condEWT = DataFormat.EffNumberForTemp(MeasurementSystem.TempConvertor(65, "IP", unit).ToString());
                }
            }
            else if (pointType == PointTypeConstants.m_cstrGBConditon)//工况下拉框引用时改索引为常量字符串 
            {
                if (percentageOfLoad == 1.0)
                {
                    condEWT = DataFormat.EffNumberForTemp(MeasurementSystem.TempConvertor(30, "SI", unit).ToString());
                }
                else if (percentageOfLoad == 0.75)
                {
                    condEWT = DataFormat.EffNumberForTemp(MeasurementSystem.TempConvertor(26, "SI", unit).ToString());
                }
                else if (percentageOfLoad == 0.5)
                {
                    condEWT = DataFormat.EffNumberForTemp(MeasurementSystem.TempConvertor(23, "SI", unit).ToString());
                }
                else if (percentageOfLoad == 0.25)
                {
                    condEWT = DataFormat.EffNumberForTemp(MeasurementSystem.TempConvertor(19, "SI", unit).ToString());
                }
            }

            else if (pointType == PointTypeConstants.m_cstrCCEWT)//工况下拉框引用时改索引为常量字符串
            {
                condEWT = DataFormat.EffNumberForTemp(Convert.ToDouble(In_CondEWT).ToString());

            }
            return condEWT;
        }

        //工况下拉框引用时改索引为常量字符串 
        public string getCondLWT(int i, int pointCount, float condLWTFullLoad, string pointType)
        {
            string condLWT = DataFormat.EffNumberForTemp(condLWTFullLoad.ToString());
            double percentageOfLoad = Math.Round(1.0 / pointCount * (pointCount - i + 1), 2);
            if (pointType == PointTypeConstants.m_cstrAHRI)//工况下拉框引用时改索引为常量字符串 "AHRI(550/590) Condenser Relief"
            {
                if (percentageOfLoad == 1.0)
                {
                    condLWT =MeasurementSystem.TempConvertor(94.30, "IP", unit).ToString("F2");
                }
                else
                {
                    condLWT = "";
                }
            }
            else
            {

            }
            return condLWT;
        }

        //工况下拉框引用时改索引为常量字符串 
        public string getEvapFlow(int i, int pointCount, float evapFlowFullLoad,string In_Capacity,string In_VPF_Evaporator_MinPct, string pointType)
        {
            string evapFlow = DataFormat.EffNumberForTemp(evapFlowFullLoad.ToString());
            double percentageOfLoad = Math.Round(1.0 / pointCount * (pointCount - i + 1), 2);
            //工况下拉框引用时改索引为常量字符串 
            if (pointType == PointTypeConstants.m_cstrVPF || pointType == PointTypeConstants.m_cstrVPFAHRICR)//20201119 增加VPF Condenser relief
            {
                if (percentageOfLoad == 1.0)
                {
                    evapFlow = DataFormat.EffNumber(double.Parse(evapFlow), 4);
                }
                else if (percentageOfLoad < 1 && (percentageOfLoad >= double.Parse(In_VPF_Evaporator_MinPct) / 100))
                {
                    evapFlow = DataFormat.EffNumber((evapFlowFullLoad * percentageOfLoad), 4);
                }
                else
                {
                    evapFlow = DataFormat.EffNumber(evapFlowFullLoad * double.Parse(In_VPF_Evaporator_MinPct) / 100, 4);
                }
            }
            if (pointType == PointTypeConstants.m_cstrGBConditon)//工况下拉框引用时改索引为常量字符串 
            {
                evapFlow = DataFormat.EffNumber(MeasurementSystem.FlowConvertor(MeasurementSystem.CapacityConvertor(double.Parse(In_Capacity), unit, "SI") * StandardSettings.EvapFlowFactorSI, "SI", unit), 4);
            }

            return evapFlow;
        }

        //工况下拉框引用时改索引为常量字符串 
        public string getCondFlow(int i, int pointCount, float condFlowFullLoad,string In_Capacity,string In_VPF_Condenser_MinPct, string pointType)
        {
            string condFlow = DataFormat.EffNumberForTemp(condFlowFullLoad.ToString());
            double percentageOfLoad = Math.Round(1.0 / pointCount * (pointCount - i + 1), 2);
            //工况下拉框引用时改索引为常量字符串 
            if (pointType == PointTypeConstants.m_cstrVPF || pointType == PointTypeConstants.m_cstrVPFAHRICR)//20201119 增加VPF Condenser relief
            {
                if (percentageOfLoad == 1.0)
                {
                    condFlow = DataFormat.EffNumber(double.Parse(condFlow), 4);
                }
                else if (percentageOfLoad < 1 && (percentageOfLoad >= double.Parse(In_VPF_Condenser_MinPct) / 100))
                {
                    condFlow = DataFormat.EffNumber((condFlowFullLoad * percentageOfLoad), 4);
                }
                else
                {
                    condFlow = DataFormat.EffNumber(condFlowFullLoad * double.Parse(In_VPF_Condenser_MinPct) / 100, 4);
                }
            }
            //工况下拉框引用时改索引为常量字符串 
            if (pointType == PointTypeConstants.m_cstrGBConditon)
            {
                condFlow = DataFormat.EffNumber(MeasurementSystem.FlowConvertor(MeasurementSystem.CapacityConvertor(double.Parse(In_Capacity), unit, "SI") * StandardSettings.CondFlowFactorSI, "SI", unit), 4);
            }
            else
            {
                condFlow = DataFormat.EffNumber(double.Parse(condFlow));
            }
            return condFlow;
        }


        #endregion

        /// <summary>
        /// GB工况 pass计算
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="efficiency"></param>
        /// <param name="iplv"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public int CalculationEfficiency(string capacity, float efficiency, double iplv,string unit)
        {
            int grade = 0;
            double capacity_GB = MeasurementSystem.CapacityConvertor(Convert.ToDouble(capacity), "SI", unit);
            //float efficiency = chilleroutputlsit[0].Efficiency.GetValueInBaseUnitsOfMeasureAsFloat();
            //double iplv = chilleroutputlsit[0].IPLV_SI;
            if (capacity_GB > 1163)
            {
                if (efficiency >= 6.30 && iplv >= 5.90)
                {
                    grade = 1;
                }
                else if (efficiency >= 5.20 && iplv >= 8.10)
                {
                    grade = 1;
                }
                else if ((efficiency >= 5.80 && efficiency < 6.30) && (iplv >= 5.90 && iplv < 8.10))
                {
                    grade = 2;
                }
                else if ((efficiency >= 5.20 && efficiency < 6.30) && (iplv >= 7.60 && iplv < 8.10))
                {
                    grade = 2;
                }
                else if ((efficiency >= 5.20 && efficiency < 5.80) && (iplv >= 5.90 && iplv < 7.60))
                {
                    grade = 3;
                }
                else
                {
                    grade = 0;
                }
            }
            else if (capacity_GB >= 528 && capacity_GB <= 1163)
            {
                if (efficiency >= 6.00 && iplv >= 5.50)
                {
                    grade = 1;
                }
                else if (efficiency >= 4.70 && iplv >= 7.50)
                {
                    grade = 1;
                }
                else if ((efficiency >= 5.60 && efficiency < 6.00) && (iplv >= 5.50 && iplv < 7.50))
                {
                    grade = 2;
                }
                else if ((efficiency >= 4.70 && efficiency < 6.00) && (iplv >= 7.00 && iplv < 7.50))
                {
                    grade = 2;
                }
                else if ((efficiency >= 4.70 && efficiency < 5.60) && (iplv >= 5.50 && iplv < 7.00))
                {
                    grade = 3;
                }
                else
                {
                    grade = 0;
                }
            }
            else if (capacity_GB <= 528)//20210325 增加528kW以下冷量能效等级判断
            {
                if (efficiency >= 5.60 && iplv >= 5.00)
                {
                    grade = 1;
                }
                else if (efficiency >= 4.20 && iplv >= 7.20)
                {
                    grade = 1;
                }
                else if ((efficiency >= 5.30 && efficiency < 5.60) && (iplv >= 5.00 && iplv < 7.20))
                {
                    grade = 2;
                }
                else if ((efficiency >= 4.20 && efficiency < 5.60) && (iplv >= 6.30 && iplv < 7.20))
                {
                    grade = 2;
                }
                else if ((efficiency >= 4.20 && efficiency < 5.30) && (iplv >= 5.00 && iplv < 6.30))
                {
                    grade = 3;
                }
                else
                {
                    grade = 0;
                }
            }
            else
            {
                grade = 0;
            }

            return grade;
        }

        /// <summary>
        /// 根据 partLoadType 返回iplv
        /// </summary>
        /// <param name="partLoadType"></param>
        /// <returns></returns>
        public string getIPLVTitle(string partLoadType)
        {
            string iplvTitle = ResultDataTitle.NPLVIP;

            switch (partLoadType)
            {
                case PointTypeConstants.m_cstrAHRI:
                    iplvTitle = ParameterList.IPLVIP;
                    break;
                case PointTypeConstants.m_cstrGBConditon:
                    iplvTitle = ResultDataTitle.IPLVGB;
                    break;
                default:
                    iplvTitle = ResultDataTitle.NPLVIP;
                    break;
            }

            return iplvTitle;
        }
    }

    #region 公英制单位转换
    public class MeasurementSystem
    {
        static MeasurementSystem()
        {
            UnitTransDic.Add("kW", "tonR");
            UnitTransDic.Add("l/s", "gpm");
            UnitTransDic.Add("℃", "℉");
            UnitTransDic.Add("℃·m²/kW", "h·ft²·℉/Btu");  //°C·m²/kW
            UnitTransDic.Add("MPa", "psig");
            UnitTransDic.Add("kPa", "ft H₂O");
            UnitTransDic.Add("kW/kW", "kW/tonR");
            UnitTransDic.Add("m/s", "ft/s");

            UnitTransDic.Add("tonR", "kW"); //Pressure Drop(ft H2O)
            UnitTransDic.Add("gpm", "l/s");
            UnitTransDic.Add("℉", "℃");
            UnitTransDic.Add("h·ft²·℉/Btu", "℃·m²/kW");
            UnitTransDic.Add("psig", "MPa");
            UnitTransDic.Add("ft H₂O", "kPa");
            UnitTransDic.Add("kW/tonR", "kW/kW");
            UnitTransDic.Add("ft/s", "m/s");
        }
        public static Dictionary<string, string> UnitTransDic = new Dictionary<string, string>();
        /// <summary>
        /// 流量关联冷量
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double CapacityToEvaFlow(double capacity, string unit)
        {
            return capacity * MeasurementSystem.getDefaultEvapFlow(unit);
        }

        /// <summary>
        /// 流量关联冷量
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static double CapacityToCondFlow(double capacity, string unit)
        {
            return capacity * MeasurementSystem.getDefaultCondFlow(unit);
        }

        /// <summary>
        /// 冷量公英制转换
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <returns></returns>
        public static double CapacityConvertor(double capacity, string fromUnit, string toUnit)
        {
            double value = capacity;
            if (fromUnit == toUnit)
                value = capacity;
            else
            {
                if (toUnit == "IP")
                    value = capacity / StandardSettings.capacityFactor;
                else
                    value = capacity * StandardSettings.capacityFactor;
            }
            return value;
        }


        public static double EfficiencyConvertor(double efficiency, string fromUnit, string toUnit)
        {
            double value = efficiency;
            if (fromUnit == toUnit)
                value = efficiency;
            else
            {
                value = StandardSettings.capacityFactor / efficiency;
            }
            return value;
        }


        /// <summary>
        /// 温度转换
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <returns></returns>
        public static double TempConvertor(double temp, string fromUnit, string toUnit)
        {
            double value = temp;
            if (fromUnit == toUnit)
                value = temp;
            else
            {
                if (toUnit == "IP")
                    value = temp * 1.8 + 32;
                else
                    value = (temp - 32) / 1.8;
            }
            return value;
        }

        public static double DeltaTempConvertor(double temp, string fromUnit, string toUnit)
        {
            double value = temp;
            if (fromUnit == toUnit)
                value = temp;
            else
            {
                if (toUnit == "IP")
                    value = temp * 1.8;
                else
                    value = temp / 1.8;
            }
            return value;
        }

        public static double FlowConvertor(double flow, string fromUnit, string toUnit)
        {
            double value = flow;
            if (fromUnit == toUnit)
                value = flow;
            else
            {
                if (toUnit == "IP")
                    value = flow / StandardSettings.flowFactor;
                else
                    value = flow * StandardSettings.flowFactor;
            }
            return value;
        }

        public static double FoulConvertor(double foul, string fromUnit, string toUnit)
        {
            double value = foul;
            if (fromUnit == toUnit)
                value = foul;
            else
            {
                if (toUnit == "IP")
                    value = foul / StandardSettings.foulFactor;
                else
                    value = foul * StandardSettings.foulFactor;
            }
            return value;
        }

        public static double PressureConvertor(double pressure, string fromUnit, string toUnit)
        {
            double value = pressure;
            if (fromUnit == toUnit)
                value = pressure;
            else
            {
                if (toUnit == "IP")
                    value = pressure / StandardSettings.pressureFactor;
                else
                    value = pressure * StandardSettings.pressureFactor;
            }
            return value;
        }

        public static double PressureDropConvertor(double pressureDrop, string fromUnit, string toUnit)
        {
            double value = pressureDrop;
            if (fromUnit == toUnit)
                value = pressureDrop;
            else
            {
                if (toUnit == "IP")
                    value = pressureDrop * StandardSettings.pressureDropFactor / 12;
                else
                    value = pressureDrop / StandardSettings.pressureDropFactor * 12;
            }
            return value;
        }

        public static double VelocityConvertor(double velocity, string fromUnit, string toUnit)
        {
            double value = velocity;
            if (fromUnit == toUnit)
                value = velocity;
            else
            {
                if (toUnit == "IP")
                    value = velocity / StandardSettings.velocityFactor;
                else
                    value = velocity * StandardSettings.velocityFactor;
            }
            return value;
        }

        public static string DesignPressureConvertor(string pressure, string fromUnit, string toUnit)
        {
            string value = pressure;
            if (fromUnit == toUnit)
                value = pressure;
            else
            {
                if (toUnit == "IP")
                {
                    if (pressure == "1.0")
                        value = "150";
                    else if (pressure == "1.6")
                        value = "232";
                    else if (pressure == "2.0")
                        value = "300";
                }
                else
                {
                    if (pressure == "150")
                        value = "1.0";
                    else if (pressure == "232")
                        value = "1.6";
                    else if (pressure == "300")
                        value = "2.0";
                }
            }
            return value;
        }

        public static double WeightConvertor(double weight, string fromUnit, string toUnit)
        {
            double value = weight;
            if (fromUnit == toUnit)
                value = weight;
            else
            {
                if (toUnit == "IP")
                    value = weight / StandardSettings.weightFactor;
                else
                    value = weight * StandardSettings.weightFactor;
            }
            return value;
        }

        public static double getDefaultEvapFoulFactor(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapfoulFactorIP;
            else
                value = StandardSettings.EvapfoulFactorSI;
            return value;
        }

        public static double getDefaultEvapEWT(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapEWTIP;
            else
                value = StandardSettings.EvapEWTSI;
            return value;
        }
        public static double getDefaultEvapEWTWPSWater(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapEWTWPSWaterIP;
            else
                value = StandardSettings.EvapEWTWPSWaterSI;
            return value;
        }
        public static double getDefaultEvapEWTWPSSource(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapEWTWPSSourceIP;
            else
                value = StandardSettings.EvapEWTWPSSourceSI;
            return value;
        }
        public static double getDefaultEvapEWTWTCRWater(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.WTCREvapEWTIP;
            else
                value = StandardSettings.WTCREvapEWTSI;
            return value;
        }
        public static double getDefaultEvapLWT(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapLWTIP;
            else
                value = StandardSettings.EvapLWTSI;
            return value;
        }

        public static double getDefaultEvapLWTWTCRWater(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.WTCREvapLWTIP;
            else
                value = StandardSettings.WTCREvapLWTSI;
            return value;
        }
        public static double getDefaultEvapFlow(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapFlowFactorIP;
            else
                value = StandardSettings.EvapFlowFactorSI;
            return value;
        }
        public static double getDefaultEvapFlowTSC(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapFlowFactorTSCIP;
            else
                value = StandardSettings.EvapFlowFactorTSCSI;
            return value;
        }
        public static double getDefaultEvapFlowHSC(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapFlowFactorHSCIP;
            else
                value = StandardSettings.EvapFlowFactorHSCSI;
            return value;
        }

        public static double getDefaultEvapFlowWPSWater(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapFlowFactorWPSWaterIP;
            else
                value = StandardSettings.EvapFlowFactorWPSWaterSI;
            return value;
        }
        public static double getDefaultEvapFlowWPSSource(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.EvapFlowFactorWPSSourceIP;
            else
                value = StandardSettings.EvapFlowFactorWPSSourceSI;
            return value;
        }
        public static double getDefaultCondEWT(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondEWTIP;
            else
                value = StandardSettings.CondEWTSI;
            return value;
        }
        public static double getDefaultCondEWTHSC(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.HSCCondEWTIP;
            else
                value = StandardSettings.HSCCondEWTSI;
            return value;
        }
        public static double getDefaultCondEWTWPSHeat(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondEWTWPSHeatIP;
            else
                value = StandardSettings.CondEWTWPSHeatSI;
            return value;
        }
        public static double getDefaultCondEWTWCC(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.WCCCondEWTIP;
            else
                value = StandardSettings.WCCCondEWTSI;
            return value;
        }
        public static double getDefaultCondEWTWPSSourceCool(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondEWTWPSSourceCoolIP;
            else
                value = StandardSettings.CondEWTWPSSourceCoolSI;
            return value;
        }
        public static double getDefaultCondEWTWPSWaterCool(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondEWTWPSWaterCoolIP;
            else
                value = StandardSettings.CondEWTWPSWaterCoolSI;
            return value;
        }
        public static double getDefaultCondEWTWTCRHeat(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.WTCRCondEWTIP;
            else
                value = StandardSettings.WTCRCondEWTSI;
            return value;
        }
        public static double getDefaultCondLWT(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondLWTIP;
            else
                value = StandardSettings.CondLWTSI;
            return value;
        }
        public static double getDefaultCondLWTHSC(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.HSCCondLWTIP;
            else
                value = StandardSettings.HSCCondLWTSI;
            return value;
        }
        public static double getDefaultCondLWTWPSWater(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondLWTWPSWaterIP;
            else
                value = StandardSettings.CondLWTWPSWaterSI;
            return value;
        }
        public static double getDefaultCondLWTWPSVWater(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondLWTWPSWaterIP;
            else
                value = StandardSettings.CondLWTWPSWaterSI;
            return value;
        }
        public static double getDefaultCondLWTWPSSource(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondLWTWPSSourceIP;
            else
                value = StandardSettings.CondLWTWPSSourceSI;
            return value;
        }
        public static double getDefaultCondLWTWPSHeat(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondLWTWPSHeatIP;
            else
                value = StandardSettings.CondLWTWPSHeatSI;
            return value;
        }
        public static double getDefaultCondLWTWPSVHeat(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondLWTWPSVHeatIP;
            else
                value = StandardSettings.CondLWTWPSVHeatSI;
            return value;
        }
        public static double getDefaultCondLWTWTCRHeat(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.WTCRCondLWTIP;
            else
                value = StandardSettings.WTCRCondLWTSI;
            return value;
        }
        public static double getDefaultCondLWTWCC(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.WCCCondLWTIP;
            else
                value = StandardSettings.WCCCondLWTSI;
            return value;
        }
        public static double getDefaultCondFlow(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondFlowFactorIP;
            else
                value = StandardSettings.CondFlowFactorSI;
            return value;
        }
        public static double getDefaultCondFlowTSC(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondFlowFactorTSCIP;
            else
                value = StandardSettings.CondFlowFactorTSCSI;
            return value;
        }
        public static double getDefaultCondFlowHSC(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondFlowFactorHSCIP;
            else
                value = StandardSettings.CondFlowFactorHSCSI;
            return value;
        }
        public static double getDefaultCondFlowWPSWater(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondFlowFactorWPSWaterIP;
            else
                value = StandardSettings.CondFlowFactorWPSWaterSI;
            return value;
        }
        public static double getDefaultCondFlowWPSSource(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondFlowFactorWPSSourceIP;
            else
                value = StandardSettings.CondFlowFactorWPSSourceSI;
            return value;
        }
        public static double getDefaultCondFoulFactor(string toUnit)
        {
            double value = 0.0f;
            if (toUnit == "IP")
                value = StandardSettings.CondfoulFactorIP;
            else
                value = StandardSettings.CondfoulFactorSI;
            return value;
        }
    }
    public class StandardSettings
    {
        public const double capacityFactor = 3.51685284206667f;
        public const double flowFactor = 0.0630901964;
        public const double foulFactor = 176.110225314329;
        public const double pressureFactor = 6.894757293;
        public const double velocityFactor = 0.3048;
        public const double weightFactor = 0.45359237;
        public const double pressureDropFactor = 4.01474213311279;
        public const double basePressureSI = 101.3f;
        public const double CapacitySIWXE = 800.0f;
        public const double CapacitySIWXEMQ = 2462.0f;
        public const double CapacitySIWSC = 4220.0f;
        public const double CapacitySIWTC = 5275.0f;  //20210104 XST 新增wtc初始冷量

        public const double EvapEWTIP = 54.00f;
        public const double EvapEWTWPSWaterIP = 59.00f;
        public const double EvapEWTWPSSourceIP = 59.00f;
        public const double EvapLWTIP = 44.00f;
        public const double EvapFlowFactorIP = 2.4f;
        public const double EvapFlowFactorTSCIP = 1.87f;
        public const double EvapFlowFactorHSCIP = 2.4f;
        public const double EvapFlowFactorWPSWaterIP = 1.437f;
        public const double EvapFlowFactorWPSSourceIP = 3f;
        public const double CondEWTIP = 85.00f;
        public const double CondEWTWPSSourceCoolIP = 64.40f;
        public const double CondEWTWPSWaterCoolIP = 77.00f;
        public const double CondLWTIP = 94.30f;
        public const double CondLWTWPSWaterIP = 113.00f;
        public const double CondLWTWPSSourceIP = 113.00f;
        public const double CondEWTWPSHeatIP = 107.60f;
        public const double CondLWTWPSHeatIP = 122.00f;
        public const double CondLWTWPSVHeatIP = 122.00f;
        public const double CondFlowFactorIP = 3.0f;
        public const double CondFlowFactorTSCIP = 2.4f;
        public const double CondFlowFactorHSCIP = 3.0f;
        public const double CondFlowFactorWPSWaterIP = 2.4f;
        public const double CondFlowFactorWPSSourceIP = 2.4f;
        public const double EvapfoulFactorIP = 0.0001f;
        public const double CondfoulFactorIP = 0.00025f;

        public const double EvapEWTSI = 12.00f;
        public const double EvapEWTWPSWaterSI = 15.00f;
        public const double EvapEWTWPSSourceSI = 15.00f;
        public const double EvapLWTSI = 7.00f;
        public const double EvapLWTWPSWaterSI = 45.00f;
        public const double EvapLWTWPSSourceSI = 45.00f;
        public const double EvapFlowFactorSI = 0.172 / 3.6;
        public const double EvapFlowFactorTSCSI = 0.134 / 3.6;
        public const double EvapFlowFactorHSCSI = 0.172 / 3.6;
        public const double EvapFlowFactorWPSWaterSI = 0.103 / 3.6;
        public const double EvapFlowFactorWPSSourceSI = 0.215 / 3.6;
        public const double CondEWTSI = 30.00f;
        public const double CondEWTWPSSourceCoolSI = 18.00f;
        public const double CondEWTWPSWaterCoolSI = 25.00f;
        public const double CondLWTSI = 35.00f;
        public const double CondLWTWPSWaterSI = 45.00f;
        public const double CondLWTWPSSourceSI = 45.00f;
        public const double CondEWTWPSHeatSI = 42.00f;
        public const double CondLWTWPSHeatSI = 50.00f;
        public const double CondLWTWPSVHeatSI = 45.00f;
        public const double CondFlowFactorSI = 0.215 / 3.6;
        public const double CondFlowFactorTSCSI = 0.172 / 3.6;
        public const double CondFlowFactorHSCSI = 0.215 / 3.6;
        public const double CondFlowFactorWPSWaterSI = 0.172 / 3.6;
        public const double CondFlowFactorWPSSourceSI = 0.172 / 3.6;

        public const double GBGroundWaterCondFlowFactorSI = 0.103 / 3.6;//WPS转移 [20220312 luo.wenmin@mcquay.com.cn]
        public const double GBGroundSourceCondFlowFactorSI = 0.215 / 3.6;//WPS转移 [20220312 luo.wenmin@mcquay.com.cn]

        public const double EvapfoulFactorSI = 0.0180f;
        public const double CondfoulFactorSI = 0.0440f;

        public const double EvapEWTSI_India = 12.00f;
        public const double EvapLWTSI_India = 7.00f;
        public const double CondEWTSI_India = 30.00f;
        public const double CondLWTSI_India = 35.00f;

        public const double CapacityHighSI = 6000;
        public const double CapacityLowSI = 350;
        public const double CapacityLowSI_520 = 150;
        public const double CapacityLowSI_TT400ForQ = 1407;
        public const double CapacityLowIP_TT400ForQ = 400;
        public const double WMTCapacityHighSI = 2128;   //2018/12/5 新增WMT限制  //20190708 XST WMT模型更新  //20200703确保600RT可以跑出
        public const double WMTCapacityLowSI = 1055;  //2018/12/5 新增WMT限制
        public const double WMT800CapacityHighIP = 10000;   //20201119 WMT800RT//20220915 XST 冷量上限取消
        public const double WMT650CapacityHighIP = 901;   //20230330 WMT650RT
        public const double WMT800CapacityLowIP = 599;   //20201119 WMT800RT
        public const double WMT650CapacityLowIP = 299;   //20230330 WMT650RT
        public const double EvapEWTHighSI = 30;//2018/11/17 蒸发器进水温度调整
        public const double EvapEWTHighSI_HP = 21;//2018/11/17 蒸发器进水温度调整
        public const double EvapEWTLowSI = 4;
        public const double WMTEvapEWTHighSI = 35; //2018/12/5 新增WMT限制//20230509 从30调整为35
        public const double WMTEvapEWTLowSI = 4;   //2018/12/5 新增WMT限制  //20190708 XST WMT模型更新  //20191128 XST
        public const double EvapLWTHighSI = 20;
        public const double EvapLWTLowSI = 3;
        public const double WMTEvapLWTHighSI = 25;  //2018/12/5 新增WMT限制//20221207 从18调整到25  
        public const double WMT800EvapLWTHighSI = 9;  //20201123 WMT800RT 
        public const double WMTEvapLWTLowSI = 4;    //2018/12/5 新增WMT限制  20190708 XST WMT模型更新/  /20191128 XST
        public const double WMT800EvapLWTLowSI = 5;  //20201123 WMT800RT
        public const double EvapFlowHighSI = 1000;
        public const double EvapFlowLowSI = 1;
        public const double CondEWTHighSI = 40.56;  //2018/11/17 冷凝器进水温度调整
        public const double CondEWTHighSI_HP = 45;  //2018/11/17 冷凝器进水温度调整
        public const double CondLWTHighSI_HP = 50;
        public const double WTCCondEWTHighSI = 40.56; //20210809 XST 冷凝器进水最高温度40 //20220302 修改为40.56
        public const double CondEWTHighIP = 105;  //2018/9/3 根据AHRI550/590对冷凝器进水温度限制  XST
        public const double CondEWTLowSI = 12.78;  //2018/11/17 冷凝器进水温度调整
        public const double CondEWTLowSI_HP = 17.78;  //2018/11/17 冷凝器进水温度调整
        public const double CondLWTLowSI_HP = 22;
        public const double WTCCondEWTLowSI = 12;   //20210809 XST 冷凝器进水最低温度40
        public const double WMTCondEWTHighIP = 95;  //2018/12/5 新增WMT限制  20190708 XST WMT模型更新    //20191128 XST
        public const double WMTCondEWTHighSI = 35;   //2018/12/5 新增WMT限制  20190708 XST WMT模型更新  //20191128 XST//20201119从38改为35
        public const double WMTCondEWTLowSI = 12.78;   //2018/12/5 新增WMT限制  20190708 XST WMT模型更新  //20191128 XST //20220406 XST 从15改为12.78
        public const double WMTCondEWTLowIP = 59;  //2018/12/5 新增WMT限制  20190708 XST WMT模型更新   //20191128 XST
        public const double WMTCondLWTLowSI = 12.78; //2018/12/5 新增WMT限制  20190708 XST WMT模型更新  //20191128 XST//20220406 XST 从15改为12.78
        public const double WMTCondLWTHighSI = 41.1; //2018/12/5 新增WMT限制  20190708 XST WMT模型更新  //20191128 XST//20230509 XST 从40改为41.1
        public const double CondEWTLowIP = 55;   //2018/9/3 根据AHRI550/590对冷凝器进水温度限制
        public const double CondLWTHighSI = 45;//2018/11/17 冷凝器出水温度调整
        public const double WXEGEN2CondLWTHighSI = 43;//2018/11/17 冷凝器出水温度调整
        public const double CondLWTLowSI = 17;
        public const double CondFlowHighSI = 1000;
        public const double CondFlowLowSI = 1;
        #region 输入限制 huang.wenkang 20210125
        public const double ScrewEvapEWTHighSI = 25;
        public const double ScrewEvapEWTHighSI_HR = 25;
        public const double ScrewEvapEWTHighSIForModule = 20;
        public const double ScrewEvapEWTLowSI = 5;
        public const double ScrewEvapLWTHighSI = 20;
        public const double ScrewEvapLWTHighSI_HR = 20;
        public const double ScrewEvapLWTHighSIForModule = 15;
        public const double ScrewEvapLWTLowSI = 4;
        public const double ScrewEvapLWTLowSI_HR = 3;
        public const double ScrewEvapLWTHighForCustomSI = 20;
        public const double ScrewEvapLWTLowForCustomSI = 2;
        public const double ScrewCondEWTHighSI = 40;
        public const double ScrewCondEWTHigh2SI = 45;
        public const double ScrewCondEWTHigh3SI = 55;
        public const double ScrewCondEWTHigh4SI = 62;
        public const double ScrewCondEWTLowSI = 15;
        public const double ScrewCondLWTHighSI = 40;
        public const double ScrewCondLWTHighSIZUW = 41;
        public const double ScrewCondLWTHigh2SI = 45;
        public const double ScrewCondLWTHigh3SI = 55;
        public const double ScrewCondLWTHigh4SI = 65;
        public const double ScrewCondLWTHighSI_CustomSelection = 45.5;
        public const double ScrewCondLWTLowSI = 20;
        #endregion
        public const double WPSEvapEWTHighSI = 20;
        public const double WPSVEvapEWTHighSI = 20;
        public const double WPSEvapEWTLowSI = 8;
        public const double WPSVEvapEWTLowSI = 8;
        public const double WPSGlycolEvapEWTLowSI = -4;
        public const double PFSGlycolEvapEWTLowSI = -7;
        public const double PFSGlycolEvapEWTHighSI = 20;
        public const double PFSGlycolEvapLWTHighSI = 15;
        public const double PFS_CLGlycolEvapEWTLowSI = -10;
        public const double PFS_CLGlycolEvapEWTHighSI = 20;
        public const double PFS_CLGlycolEvapLWTLowSI = -10;
        public const double PFS_CLGlycolEvapLWTHighSI = 15;
        public const double PFS_CLGlycolEvapEWTLow2SI = -8;
        public const double PFS_CLGlycolEvapEWTHigh2SI = 20;
        public const double PFS_CLGlycolEvapLWTLow2SI = -8;
        public const double PFS_CLGlycolEvapLWTHigh2SI = 15;
        public const double WPSGlycolEvapEWTHighSI = 20;
        public const double WPSGlycolEvapLWTHighSI = 15;
        public const double PFSGlycolEvapLWTLowSI = -11;
        public const double WPSGlycolEvapLWTLowSI = -6;
        public const double WPSEvapLWTHighSI = 15;
        public const double WPSEvapLWTLowSI = 4;
        public const double WPSVEvapLWTHighSI = 15;
        public const double WPSVEvapLWTLowSI = 4;
        public const double WPSCondEWTHighSI = 65;
        public const double WPSCondEWTLowSI = 35;
        public const double WPSVCondEWTHighSI = 65;
        public const double WPSVCondEWTLowSI = 15;
        public const double WPSCondLWTHighSI = 65;
        public const double WPSCondLWTHighSI_V = 45.5;
        public const double WPSCondLWTLowSI = 16.5;
        public const double WPSVCondLWTHighSI = 65;
        public const double WPSVCondLWTLowSI = 16.5;
        public const double WPSCoolCondEWTHighSI = 40;
        public const double WPSCoolCondEWTLowSI = 16.5;
        public const double WPSCoolCondLWTHighSI = 40;
        public const double WPSCoolCondLWTLowSI = 16.5;
        #region WLT [20211206 luo.wenmin@mcquay.com.cn]
        public const double WLTCapacityHighSI2000 = 10550; //WLT2000制冷量上限10550(3000RT)
        public const double WLTCapacityHighSI = 3166; //WLT制冷量上限
        public const double WLTCapacityLowSI = 2461;  //WLT制冷量下限
        public const double WLTEvapEWTHighSI = 30;  //WLT蒸发器进水温度上限
        public const double WLTEvapEWTLowSI = 3;   //WLT蒸发器进水温度下限
        public const double WLTEvapLWTHighSI2000 = 25;  //WLT2000蒸发器出水温度上限25
        public const double WLTEvapLWTHighSI = 18;  //WLT蒸发器出水温度上限
        public const double WLTEvapLWTLowSI = 3;   //WLT蒸发器出水温度下限

        public const double WLTCondEWTHighSI = 35;  //WLT冷凝器进水温度上限
        public const double WLTCondEWTLowSI = 15;   //WLT冷凝器进水温度下限
        public const double WLTCondLWTHighSI = 40;  //WLT冷凝器出水温度上限
        public const double WLTCondLWTLowSI = 15;   //WLT冷凝器出水温度下限
        #endregion

        #region TSC HWK 20230224
        public const double TSCEvapFlowSI = 201.6;
        public const double TSCCondFlowSI = 252;
        public const double TSCEvapEWTLowSI = 3;
        public const double TSCEvapEWTHighSI = 30;
        public const double TSCEvapLWTLowSI = 3;
        public const double TSCEvapLWTHighSI = 18;
        public const double TSCCondEWTLowSI = 12.78;
        public const double TSCCondEWTHighSI = 55;
        public const double TSCCondLWTLowSI = 12.78;
        public const double TSCCondLWTHighSI = 55;
        #endregion

        #region HSC HWK 20230421
        public const double HSCEvapEWTLowSI = 3;
        public const double HSCEvapEWTHighSI = 30;
        public const double HSCEvapLWTLowSI = 3;
        public const double HSCEvapLWTHighSI = 18;
        public const double HSCCondEWTLowSI = 12.78;
        public const double HSCCondEWTHighSI = 40.56;
        public const double HSCCondLWTLowSI = 12.78;
        public const double HSCCondLWTHighSI = 45;
        public const double HSCCondEWTSI = 32.00f;
        public const double HSCCondLWTSI = 37.00f;
        public const double HSCCondEWTIP = 89.60f;
        public const double HSCCondLWTIP = 98.60f;
        #endregion

        #region WTCR HWK 20230421
        public const double WTCREvapEWTLowSI = 3;
        public const double WTCREvapEWTHighSI = 30;
        public const double WTCREvapLWTLowSI = 3;
        public const double WTCREvapLWTHighSI = 20;
        public const double WTCRCondEWTLowSI = 12;
        public const double WTCRCondEWTHighSI = 40.56;
        public const double WTCRCondLWTLowSI = 17;
        public const double WTCRCondLWTHighSI = 46;
        public const double WTCREvapEWTSI = 10.00f;
        public const double WTCREvapLWTSI = 5.00f;
        public const double WTCREvapEWTIP = 50.00f;
        public const double WTCREvapLWTIP = 41.00f;
        public const double WTCRCondEWTSI = 40.00f;
        public const double WTCRCondLWTSI = 45.00f;
        public const double WTCRCondEWTIP = 104.00f;
        public const double WTCRCondLWTIP = 113.00f;
        #endregion

        #region WCC XST  20230822
        public const double WCCCondEWTSI = 32.00f;
        public const double WCCCondLWTSI = 37.00f;
        public const double WCCCondEWTIP = 89.60f;
        public const double WCCCondLWTIP = 98.60f;

        #endregion

        #region HeatExchanger HWK 20230906
        public const double HXGlycolEvapEWTHighSI = 20;
        public const double HXGlycolEvapEWTLowSI = -7;
        public const double HXEvapEWTHighSI = 25;
        public const double HXEvapEWTLowSI = 5;

        public const double HXGlycolEvapLWTHighSI = 20;
        public const double HXGlycolEvapLWTLowSI = -7;
        public const double HXEvapLWTHighSI = 25;
        public const double HXEvapLWTLowSI = 5;

        //public const double HXGlycolCondEWTHighSI = 20;
        //public const double HXGlycolCondEWTLowSI = -7;
        public const double HXCondEWTHighSI = 40;
        public const double HXCondEWTLowSI = 15;

        //public const double HXGlycolCondLWTHighSI = 20;
        //public const double HXGlycolCondLWTLowSI = -7;
        public const double HXCondLWTHighSI = 40;
        public const double HXCondLWTLowSI = 20;
        #endregion
    }
    public class PointTypeConstants
    {
        public const string m_cstrUserDef = "User Defined";
        public const string m_cstrGBCR = "GB Condenser Relief";
        public const string m_cstrVPFAHRICR = "VPF AHRI(550/590) Condenser Relief";
        public const string m_cstrCCEWT = "Constant Condenser EWT";
        public const string m_cstrAHRI = "AHRI(550/590)";
        public const string m_cstrAHRICR = "AHRI(550/590) Condenser Relief";
        public const string m_cstrGBConditon = "GB Condition";
        public const string m_cstrVPF = "VPF(Evap&Cond)";

    }

    public class FluidCalType
    {
        public const string flow_lwt = "FLOW+LWT";   //FluidCalType1
        public const string flow_ewt = "FLOW+EWT";
        public const string ewt_lwt = "EWT+LWT"; //FluidCalType2
    }
    #endregion

    public static class DataFormat
    {
        public static string InputPowerFomat(string source)
        {
            var result = source;
            try
            {
                string temp = source.Replace("V","").Replace("N","").Replace("Hz","");
                var templist = temp.Split('/');
                if (templist.Count() == 3)
                {
                    result = templist[2] + "/" + templist[1] + "/" + templist[0];
                }
            }
            catch (Exception ex)
            {


            }
            return result;

        }

        /// <summary>
        /// 温度截取 默认两位
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dig"></param>
        /// <returns></returns>
        public static string EffNumberForTemp(string d, int dig = 2)
        {
            if (d == "") return "";
            else if (d == "0.0") return "0.0";
            else
            {
                return d.ToPares().ToString($"F{dig}");
            }

        }
        public static string EffNumber(double d, int m = 4)
        {
            if (d > 10000)//大于10000，传入4是不合理的，因为外层调用的地方太多，于是在此处理 
            {
                m = 5;
            }
            int n = m;
            string value = "";
            //edit by ljm 
            if (m == 3)
            {
                if (d == 0) return "0.00";
                if (d == 0.0) return "0.00";
            }
            else if (m == 4)
            {
                if (d == 0) return "00.00";
                if (d == 0.0) return "00.00";
            }
            else if (m == 5) // 增加5有效位数[20190715 luo.wenmin@mcquay.com.cn]
            {
                if (d == 0 || d == 0.0) return "000.00";
            }

            //if (d == 0) return "";
            //if (d == 0.0) return "";
            //end
            if (d > 1 || d < -1)
                n = n - (int)Math.Log10(Math.Abs(d)) - 1;
            else
                n = n + (int)Math.Log10(1.0 / Math.Abs(d));
            if (n < 0)
            {
                d = (int)(d / Math.Pow(10, 0 - n)) * Math.Pow(10, 0 - n);
                n = 0;
            }
            double v = Math.Round(d, n);

            if (m == 3)
            {
                if (Math.Abs(v) >= 1000)
                    value = v.ToString();
                else if (Math.Abs(v) >= 100)
                    value = string.Format(v.ToString());
                else if (Math.Abs(v) >= 10)
                    value = string.Format(v.ToString("f1"));
                else if (Math.Abs(v) >= 1)
                    value = string.Format(v.ToString("f2"));
                else if (Math.Abs(v) >= 0.1)
                    value = string.Format(v.ToString("f3"));
                else if (Math.Abs(v) >= 0.01)
                    value = string.Format(v.ToString("f4"));
                else if (Math.Abs(v) >= 0.001)
                    value = string.Format(v.ToString("f5"));
                else if (Math.Abs(v) >= 0.0001)
                    value = string.Format(v.ToString("f6"));
                else if (Math.Abs(v) >= 0.00001)
                    value = string.Format(v.ToString("f7"));
            }
            else if (m == 4)
            {
                if (Math.Abs(v) >= 1000)
                    value = v.ToString();
                else if (Math.Abs(v) >= 100)//add abs 201903
                    value = string.Format(v.ToString("f1"));
                else if (Math.Abs(v) >= 10)
                    value = string.Format(v.ToString("f2"));
                else if (Math.Abs(v) >= 1)
                    value = string.Format(v.ToString("f3"));
                else if (Math.Abs(v) >= 0.1)
                    value = string.Format(v.ToString("f4"));
                else if (Math.Abs(v) >= 0.01)
                    value = string.Format(v.ToString("f5"));
                else if (Math.Abs(v) >= 0.001)
                    value = string.Format(v.ToString("f6"));
                else if (Math.Abs(v) >= 0.0001)
                    value = string.Format(v.ToString("f7"));
                else if (Math.Abs(v) >= 0.00001)
                    value = string.Format(v.ToString("f8"));
            }
            else if (5 == m)// 增加5有效位数
            {
                if (Math.Abs(v) >= 10000)
                    value = v.ToString();
                else if (Math.Abs(v) >= 1000)
                    value = string.Format(v.ToString("f1"));
                else if (Math.Abs(v) >= 100)
                    value = string.Format(v.ToString("f2"));
                else if (Math.Abs(v) >= 10)
                    value = string.Format(v.ToString("f3"));
                else if (Math.Abs(v) >= 1)
                    value = string.Format(v.ToString("f4"));
                else if (Math.Abs(v) >= 0.1)
                    value = string.Format(v.ToString("f5"));
                else if (Math.Abs(v) >= 0.01)
                    value = string.Format(v.ToString("f6"));
                else if (Math.Abs(v) >= 0.001)
                    value = string.Format(v.ToString("f7"));
                else if (Math.Abs(v) >= 0.0001)
                    value = string.Format(v.ToString("f8"));
                else if (Math.Abs(v) >= 0.00001)
                    value = string.Format(v.ToString("f9"));
            }
            return value;
        }
        /// <summary>
        /// 流量截取 默认4位
        /// </summary>
        /// <param name="d"></param>
        /// <param name="dig"></param>
        /// <returns></returns>
        public static string EffNumber2(double d,int dig =4 )
        {
            return d.ToString($"F{dig}");
        }


        /// <summary>
        /// 默认4位
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dig"></param>
        /// <returns></returns>
        public static string KeepDig2(string value,int dig = 4)
        {
            return value.ToPares().ToString($"F{dig}");
        }
        /// <summary>
        /// 默认4位
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dig"></param>
        /// <returns></returns>
        public static string KeepDig2(double value, int dig = 4)
        {
            return value.ToString($"F{dig}");
        }
        public static string DecNum(double d, int n)
        {
            if (d.ToString() == "") return "";
            if (d == 0) return "";
            if (d == 0.0) return "";
            else
            {
                double v = Math.Round(Convert.ToDouble(d), n);
                if (n == 1)
                    return v.ToString("f1");
                else if (n == 2)
                    return v.ToString("f2");
                else
                    return v.ToString();
            }
        }
        public static bool IsSubcoolingOver(List<ChillerOutputList> chillerOutputList)
        {

            for (int i = chillerOutputList.Count - 1; i >= 0; i--)
            {
                if (Math.Round(chillerOutputList[i].PercentageOfLoad * 100) >= 90)
                {
                    if (chillerOutputList[i].SupercoolingTemp.GetValueInBaseUnitsOfMeasureAsDouble() < 2.3)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }


    public class ResultDataTitle
    {
        public ResultDataTitle(string unit)
        {
            if (unit == "IP")
            {
                Regex regex = new Regex(@"\((.*?)\)");

            }
        }
        public string load_point_data { get; set; } = "Load Point Data:";
        public string percofFullLoad { get; set; } = "Perc. of Full Load(%)";
        public string capacity { get; set; } = "Capacity(kW)";
        public string evaewt { get; set; } = "Evaporator EWT(℃)";
        public string evaflow  { get; set; } = "Evaporator Flow(l/s)";
        public string evalwt { get; set; } = "Evaporator LWT(℃)";
        public string condlwt { get; set; } = "Condenser LWT(℃)";
        public string condflow { get; set; } = "Condenser Flow(l/s)";
        public string condewt { get; set; } = "Condenser EWT(℃)";

        public string result { get; set; } = "Results:";
        public string acapacity { get; set; } = "Actual Capacity(kW)";
        public string inputpower { get; set; } = "Power Input(kW)";
        public string COP { get; set; } = "Efficiency(kW/kW)";
        public string NPLV { get; set; } = "NPLV.IP(kW/kW)";
        public string RLA { get; set; } = "RLA(A)";
    
        public string evadata { get; set; } = "Evaporator Data:";
        public string predrop { get; set; } = "Pressure Drop(kPa)";
   
        public string conddata { get; set; } = "Condenser Data:";
      
        public string eledata { get; set; } = "Electrical Data:";
        public string MOCP { get; set; } = "MOCP(A)";
        public string powerfactor { get; set; } = "Power Factor";
        public string outputAmps { get; set; } = "OutputAmps";
        
        public string servicedata { get; set; } = "Service Data:";
        public string evatemp { get; set; } = "Evap. Temp.(℃)";
        public string evapressure { get; set; } = "Evap. Pressure(kPa)";
        public string evawater { get; set; } = "Evap. Water Vel.(m/s)";
        public string condtemp { get; set; } = "Cond. Temp.(℃)";
        public string condpressure { get; set; } = "Cond. Pressure(kPa)";
        public string condwater { get; set; } = "Cond. Water Vel.(m/s)";
        public string superheat { get; set; } = "Superheat(℃)";
        public string subcooling { get; set; } = "Subcooling(℃)";

        public string mes { get; set; } = "Message:";

        public string gb15977title { get; set; } = "Regional Standard - GB19577:";
        public string gb55015title { get; set; } = "Regional Standard - GB55015:";
        public string load_P75 { get; set; } = "75% Load(kW/kW)";
        public string load_P50 { get; set; } = "50% Load(kW/kW)";
        public string load_P25 { get; set; } = "25% Load(kW/kW)";
        public string IPLV_GB { get; set; } = "IPLV_GB(kW/kW)";

        public string PLV_NAME { get; set; } = NPLVIP;  //3中类型 同一个title


        public const string NPLVIP = "NPLV.IP(kW/kW)";

        public const string IPLVIP = "IPLV.IP(kW/kW)";

        public const string IPLVGB = "IPLV_GB(kW/kW)";

    }

    public enum CapacityType
    {
        NormalCapacity,
        GB15977Capacity,
        GB55015Capacity,
        ConstantCondenserEWT

    }

    public enum Ratingtype
    {
        Modular,
        Centrifuge,
    }

    public enum ModelType
    {
        Modular=0,
        Centrifuge
    }
}
