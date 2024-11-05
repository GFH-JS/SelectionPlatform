using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Models.Models
{
    public class ProjectInfoEntity
    {
        public ProjectInfoEntity()
        {
        }
        public ObjectId id { get; set; }
        public string projectId { get; set; }
        public string projectName { get; set; }
        public string projectVersion { get; set; }
        public int status { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime createTime { get; set; } = DateTime.Now;
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime updateTime { get; set; }
        public string metricInch { get; set; }  //SI公  IP
        public int modelType { get; set; } = 1;  //0模块机 1 离心机
        public ProofData proofData { get; set; } = new ProofData();
    }


    /// <summary>
    /// 输入 + 结果集
    /// </summary>
    public class CalculateData
    {
        public string partloadLoadPoint { get; set; }
        public string capacity { get; set; }
        public string inputPwoer { get; set; }
        public string starterType { get; set; }
        public string mountingType { get; set; }
        public string chillerCode { get; set; }
        public string evaflowEwt { get; set; }
        public string evaflow { get; set; }
        public string evaewt { get; set; }
        public string evalwt { get; set; }
        public string evapass { get; set; }
        public string evafoulingfactor { get; set; }
        public string evafluidType { get; set; }
        public string evaglycolQty { get; set; }
        public string evadesignPressure { get; set; }
        public string condflowEwt { get; set; }
        public string condflow { get; set; }
        public string condewt { get; set; }
        public string condlwt { get; set; }
        public string condpass { get; set; }
        public string condfoulingfactor { get; set; }
        public string condfluidType { get; set; }
        public string condglycolQty { get; set; }
        public string conddesignPressure { get; set; }
        public string partloadType { get; set; }
        public string count { get; set; }
        public string evaporatorVpfMin { get; set; }
        public string condenserVpfMin { get; set; }
        public string gb19577 { get; set; }
        public string gb19577Capacity { get; set; }
        public string gb55015 { get; set; }
        public string gb55015Capacity { get; set; }
        public string technicalData { get; set; }
        public string calculationType { get; set; }
        public string maxPercentLoad { get; set; }
        public string maxCewt { get; set; }
        public string maxElwt { get; set; }
        public string minPercentLoad { get; set; }
        public string minCewt { get; set; }
        public string minElwt { get; set; }
        public string stepPercentLoad { get; set; }
        public string stepCewt { get; set; }
        public string stepElwt { get; set; }

        public List<PartloadDatalist_group> partloadDatalists { get; set; } = new List<PartloadDatalist_group>();   ///可用户自定义输入值

    }

    public class ProofData
    {
        public ProofData()
        {
           
        }
        public InputParam partloadLoadPoint { get; set; }
        public InputParam capacity { get; set; }
        public InputParam capacityC { get; set; }
        public InputParam inputPwoer { get; set; }
        public InputParam starterType { get; set; }
        public InputParam starterTypeC { get; set; }
        public InputParam mountingType { get; set; }
        public InputParam chillerCode { get; set; }
        public string? chillercode_input { get; set; } //仅仅显示
        public InputParam evaflowEwt { get; set; }
        public InputParam evaflow { get; set; }
        public NumInputParam evaewt { get; set; }
        public NumInputParam evalwt { get; set; }
        public InputParam evapass { get; set; }
        public InputParam evapassC { get; set; }
        public InputParam evafoulingfactor { get; set; }
        public InputParam evafluidType { get; set; }
        public InputParam evaglycolQty { get; set; }
        public InputParam evadesignPressure { get; set; }
        public InputParam condflowEwt { get; set; }
        public InputParam condflow { get; set; }
        public NumInputParam condewt { get; set; }
        public NumInputParam condlwt { get; set; }
        public InputParam condpass { get; set; }
        public InputParam condpassC { get; set; }
        public InputParam condfoulingfactor { get; set; }
        public InputParam condfluidType { get; set; }
        public InputParam condglycolQty { get; set; }
        public InputParam conddesignPressure { get; set; }
        public InputParam partloadType { get; set; }
        public InputParam count { get; set; }
        public InputParam evaporatorVpfMin { get; set; }
        public InputParam condenserVpfMin { get; set; }
        public InputParam gb19577 { get; set; }
        public InputParam gb19577Capacity { get; set; }
        public InputParam gb55015 { get; set; }
        public InputParam gb55015Capacity { get; set; }
        public InputParam technicalData { get; set; }
        public InputParam calculationType { get; set; }
        public InputParam maxPercentLoad { get; set; }
        public InputParam maxCewt { get; set; }
        public InputParam maxElwt { get; set; }
        public InputParam minPercentLoad { get; set; }
        public InputParam minCewt { get; set; }
        public InputParam minElwt { get; set; }
        public InputParam stepPercentLoad { get; set; }
        public InputParam stepCewt { get; set; }
        public InputParam stepElwt { get; set; }
        /// 离心机rating参数 
        public InputParam chillerType { get; set; }
        public InputParam impellerFamily { get; set; }
        public InputParam impellerWidth { get; set; }
        public InputParam gear { get; set; }
        public InputParam compresserNumber { get; set; }
        public InputParam stageNumber { get; set; }
        public InputParam moter { get; set; }
        public InputParam evaType { get; set; }
        public InputParam evaDia { get; set; }
        public InputParam evaLength{ get; set; }
        public InputParam evaTubeNum { get; set; }
        public InputParam evaTubeType { get; set; }
        public InputParam condType { get; set; }
        public InputParam condDia { get; set; }
        public InputParam condLength { get; set; }
        public InputParam condTubeNum { get; set; }
        public InputParam condTubeType { get; set; }


        public List<LoadPointTitle>? loadPointTitle { get; set; }
        public List<PartloadDatalist_group> partloadDatalists { get; set; } = new List<PartloadDatalist_group>();   ///可用户自定义输入值
        public List<PartloadDatalist_group> loadPointDatalists { get; set; } = new List<PartloadDatalist_group>();
    }

    public class InputParam
    {
        public InputParam()
        {
            paramComboxValue = new List<DropDownListData>();
        }
        public string paramName { get; set; }
        public string paramValue { get; set; }
        public string paramUnit { get; set; }
        public List<DropDownListData> paramComboxValue { get; set; }
    }

    public class NumInputParam
    {
        public NumInputParam()
        {
            paramComboxValue = new List<DropDownListData>();
        }
        public string paramName { get; set; }
        public double paramValue { get; set; }
        public string paramUnit { get; set; }
        public List<DropDownListData> paramComboxValue { get; set; }
    }

    public class DropDownListData
    {
        public string label { get; set; }
        public string value { get; set; }
        public bool selected { get; set; }
    }

    public class PartloadDatalist_group
    {
        
        public string? key { get; set; }
        public string? name { get; set; }
        public string? k1 { get; set; }="";
        public string? k2 { get; set; }="";
        public string? k3 { get; set; }="";
        public string? k4 { get; set; }="";
        public string? k5 { get; set; }="";
        public string? k6 { get; set; }="";
        public string? k7 { get; set; }="";
        public string? k8 { get; set; }="";
        public string? k9 { get; set; }="";
        public string? k10 { get; set; } = "";

        //loadpoint表用
        public string? capacity { get; set; }
        public string? percentLoad { get; set; }
        public string? evapFlow { get; set; }
        public string? evapLwt { get; set; }
        public string? condFlow { get; set; }

        public List<PartloadDatalist> children = new List<PartloadDatalist>();

    }

    public class PartloadDatalist
    {
        public string? key { get; set; }
        public string? name { get; set; }   
        public string? k1 { get; set; } = "";
        public string? k2 { get; set; } ="";
        public string? k3 { get; set; } ="";
        public string? k4 { get; set; } ="";
        public string? k5 { get; set; } ="";
        public string? k6 { get; set; } ="";
        public string? k7 { get; set; } ="";
        public string? k8 { get; set; } ="";
        public string? k9 { get; set; } ="";
        public string? k10 { get; set; }="";
        public string? k11 { get; set; } = "";
        public string? k12 { get; set; } = "";
        public string? k13 { get; set; } = "";
        public string? k14 { get; set; } = "";
        public string? k15 { get; set; } = "";
        public string? k16 { get; set; } = "";
        public string? k17 { get; set; } = "";
        public string? k18 { get; set; } = "";
        public string? k19 { get; set; } = "";
        public string? k20 { get; set; } = "";
        public string? k21 { get; set; } = "";
        public string? k22 { get; set; } = "";
        public string? k23 { get; set; } = "";
        public string? k24 { get; set; } = "";
        public string? k25 { get; set; } = "";
        public string? k26 { get; set; } = "";
        public string? k27 { get; set; } = "";
        public string? k28 { get; set; } = "";
        public string? k29 { get; set; } = "";

        //loadpoint表用
        public string? capacity { get; set; }
        public string? percentLoad { get; set; }
        public string? evapFlow { get; set; }
        public string? evapLwt { get; set; }
        public string? condFlow { get; set; }
    }


    public class LoadPointTitle
    {
    
        public string? title { get; set; }
        public List<LoadPointTitleChild> ?children { get; set; }
    }

    public class LoadPointTitleChild
    {
        public string? title { get; set; }
        public string? dataIndex { get; set; }
        public string? key { get; set; }
        public double? width { get; set; }
        public List<LoadPointTitleChild> ?children { get; set; }
    }
}
