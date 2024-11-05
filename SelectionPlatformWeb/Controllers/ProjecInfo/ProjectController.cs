using Amazon.Runtime.Internal.Util;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SelectionPlatform.Configuration;
using SelectionPlatform.EntityFramework;
using SelectionPlatform.IRepository;
using SelectionPlatform.IRepository.ParamComparison;
using SelectionPlatform.IRepository.ProjectInfo;
using SelectionPlatform.IServices.City;
using SelectionPlatform.IServices.ProjectInfo;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.DTO;
using SelectionPlatform.Models.ViewModels.RequestFeatures;
using SelectionPlatform.Models.ViewModels.ResponseFeatures;
using SelectionPlatform.Repository.ParamComparison;
using SelectionPlatform.Repository.ProjectInfo;
using SelectionPlatform.Services.CentrifugeCalculate;
using SelectionPlatform.Services.ProjectInfo;
using SelectionPlatform.Utility.Extensions;
using SelectionPlatform.Utility.Helper;
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SelectionPlatformWeb.Controllers.ProjecInfo
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
       
        private readonly IMapper _mapper;
        private InitData _fakeData;
        private readonly IProjectbaseInfoServices _projectbaseInfoServices;
        private readonly CentrifugeCalculateHelper _centrifugeCalculateHelper;
        private readonly IProjectServices _projectServices;
        private readonly ICityServices _cityServices;
        private readonly IParamComparisonRepository _paramComparisonRepository;


        public ProjectController(IMapper mapper, IProjectServices projectServices,InitData fakeData, IProjectbaseInfoServices projectbaseInfoServices,ICityServices cityServices,IParamComparisonRepository paramComparisonRepository,
            CentrifugeCalculateHelper centrifugeCalculateHelper,AppSettingsHelper appSettingsHelper)
        {
            _projectServices = projectServices;
            _mapper = mapper;
            _fakeData = fakeData;
            _projectbaseInfoServices = projectbaseInfoServices;
            _centrifugeCalculateHelper = centrifugeCalculateHelper;
            _cityServices = cityServices;
            _paramComparisonRepository = paramComparisonRepository;

        }

        /// <summary>
        /// 首页 加载已保存的项目 分页返回
        /// </summary>
        /// <param name="projectQueryParameters"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProjects([FromQuery] ProjectQueryParameters projectQueryParameters)
        {
            try
            {
                //var  pp =await  _paramComparisonRepository.FindAll().Where(p => p.CategoryName == "蒸发器管数代码" && p.FromParm == "4").ToListAsync();
                var result = _projectbaseInfoServices.GetProjects(projectQueryParameters);
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.MetaData));

                var probaseinfo = _mapper.Map<IEnumerable<ProjectbaseInfoDto>>(result);
             
                foreach (var item in probaseinfo)
                {
                    item.Versions = await _projectServices.GetProjectAllVersionById(item.ID);
                }
                 var r2 = probaseinfo.ShapeData(projectQueryParameters.Fields);
 
                ProjectsResposne projectsResposne = new ProjectsResposne();
                projectsResposne.list = r2;
                projectsResposne.page = result.MetaData;
               
                return Ok(new { data = projectsResposne}); //
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }


        [HttpPost]
        public async Task<IActionResult> AddProject(AddProjectbaseInfoDto projectbaseInfo)
        {
            try
            {
                var proj = _mapper.Map<ProjectbaseinfoEntity>(projectbaseInfo);
                //using (var transaction = _mysqlDbContext.Database.BeginTransaction())
                //{
                //    await transaction.CommitAsync();
                //}
                var addcount = _projectbaseInfoServices.Insert(proj);
                //_projectbaseInfoServices.SaveChangesAsync();
                return Ok(new ApiResult());
            }
            catch (Exception ex)
            {

                return BadRequest(ex.InnerException);
            }
           
            //var proj = _mapper.Map<ProjectInfoEntity>(projectInfoEntity);
            //string oldv = proj.projectVersion;
            //string pattern = @"V(\d+)";
            //var match = Regex.Match(oldv, pattern);
            //if (match.Success)
            //{
            //    int V = int.Parse(match.Groups[1].Value);
            //    V++;
            //    proj.projectVersion = $"V{V}";
            //}
            //else
            //{
            //    proj.projectVersion = "V1";
            //}
           
            //await _projectServices.Insert(proj);
            //return Ok(projectInfoEntity);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProject(string projectId)
        {
            try
            {
                var proj = _projectbaseInfoServices.FindByExpress(p=>p.ID == projectId).FirstOrDefault();
                if (proj == null)
                {
                    return BadRequest(new ApiResult() { msg = "该用户不存在"});
                }
                proj.DEL_FLAG = "1";
                _projectbaseInfoServices.Update(proj);
                return Ok(new ApiResult());
            }
            catch (Exception ex)
            {

                return BadRequest(ex.InnerException);
            }

            //var proj = _mapper.Map<ProjectInfoEntity>(projectInfoEntity);
            //string oldv = proj.projectVersion;
            //string pattern = @"V(\d+)";
            //var match = Regex.Match(oldv, pattern);
            //if (match.Success)
            //{
            //    int V = int.Parse(match.Groups[1].Value);
            //    V++;
            //    proj.projectVersion = $"V{V}";
            //}
            //else
            //{
            //    proj.projectVersion = "V1";
            //}

            //await _projectServices.Insert(proj);
            //return Ok(projectInfoEntity);
        }

        [HttpPost]
        public async Task<IActionResult> CapacityToFlow(ProjectInputDto projectInput)
        {
            try
            {
                var re = _centrifugeCalculateHelper.CapacityToFlow(projectInput);
                return Ok(new ApiResult { data = re});  //这里返回整个数据
            }
            catch (Exception ex)
            {
               return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// 动态计算输入参数
        /// </summary>
        /// <param name="projectInfoEntity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CalculateInput(ProjectInputDto projectInput)
        {
            try
            {
                var re = _centrifugeCalculateHelper.CalculateInput(projectInput);
                return Ok(new ApiResult { data = re});  //这里返回整个数据
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }  
        }



        /// <summary>
        /// 动态计算 loadpoint 输入
        /// </summary>
        /// <param name="projectInput"></param>
        /// <returns></return
        [HttpPost]
        public async Task<IActionResult> CalculteLoadPoint(ProjectInputDto projectInput)
        {
            try
            {
                var re = _centrifugeCalculateHelper.GenerateLoadPointDto(projectInput);
                return Ok(new ApiResult { data = re });  //这里返回整个数据
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }


        /// <summary>
        /// 新项目将默认值转换后推到前端
        /// 如果已rating后的项目 ，将最新的rating 数据转换后推到前端
        /// </summary>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <param name="projectInput"></param>
        /// <returns></returns>
        //[HttpPost]
        //public async Task<IActionResult> UnitConvert(string fromUnit, string toUnit, ProjectInputDto projectInput)
        //{
        //    try
        //    {
        //        var proj = await _projectServices.GetProjectsByIdAndVersion(projectInput.projectId, projectInput.projectVersion);
        //        if (proj == null)
        //        {
        //            projectInput.proofData = _fakeData.InitInputData().proofData;
        //        }
        //        else 
        //        {
        //            projectInput.proofData = proj.proofData;
        //        }

        //        _centrifugeCalculateHelper.UnitConvert(fromUnit, toUnit, projectInput);

        //        return Ok(new ApiResult { data = projectInput });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
         
        //}

        /// <summary>
        /// 输出计算结果  rating  同时保存项目
        /// </summary>
        /// <param name="projectInfoEntity"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RatingData(ProjectInputDto projectInput)
        {
            try
            {
                var calcInput = _centrifugeCalculateHelper.CalculateInput(projectInput);  //不能拿界面的加载数据
                var result = _centrifugeCalculateHelper.RatingData(calcInput);
                if (!string.IsNullOrEmpty(result)) {
                    return Ok(new ApiResult { msg = result,code= 602 });   //
                }

                var proj = _mapper.Map<ProjectInfoEntity>(projectInput);
              
                #region 拿到最新版本
                List<string> version = new List<string>();
                var _exit_proj = await _projectServices.GetProjectsByProjectId(projectInput.projectId);
                if (_exit_proj!=null && _exit_proj.Count>0)
                {
                    _exit_proj = _exit_proj.OrderByDescending(p => p.projectVersion).ToList();
                    foreach (var item in _exit_proj)
                    {
                        version.Add(item.projectVersion);
                    }
                    string oldv = version[0];
                    string pattern = @"V(\d+)";
                    var match = Regex.Match(oldv, pattern);
                    if (match.Success)
                    {
                        int V = int.Parse(match.Groups[1].Value);
                        V++;
                        proj.projectVersion = $"V{V}";
                    }
                    else
                    {
                        proj.projectVersion = "V1";
                    }
                }
                else
                {
                    proj.projectVersion = "V1";
                }
                projectInput.projectVersion = proj.projectVersion;
           
                #endregion

                //await _projectServices.Insert(proj);
                return Ok(new ApiResult { data = projectInput });   //proj
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
          
        }

        [HttpPost]
        public async Task<IActionResult> RatingMatrix(ProjectInputDto projectInput)
        {
            try
            {
                var re = _centrifugeCalculateHelper.RatingMatrixInputData(projectInput);
                var proj = _mapper.Map<ProjectInfoEntity>(projectInput);
                return Ok(new ApiResult { data = re });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
         
        }

        /// <summary>
        /// 双击进入查询项目数据 如果没有数据 给默认输入  
        /// + 切换公英制的时候调用
        /// </summary>
        /// <param name="projectQueryParameters"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProjectDataById(string projectId,string? projectVersion,string metricInch,int modelType = 1)
        {
            try
            {
                bool needCalculate = false;
                var result = await _projectServices.GetProjectsByIdAndVersion(projectId, projectVersion);
                if (result == null)
                {
                    result = _fakeData.InitInputData();
                    if (modelType == (int)ModelType.Centrifuge)
                    {
                        result.proofData.capacity = result.proofData.capacityC;
                        result.proofData.starterType = result.proofData.starterTypeC;
                        result.proofData.evapass = result.proofData.evapassC;
                        result.proofData.condpass = result.proofData.condpassC;
                    }
                    result.modelType = modelType;
                    result.projectId = projectId;
                    result.metricInch = "SI";
                    result.projectVersion = "";

                    result.proofData.evaflow.paramValue = DataFormat.EffNumber(MeasurementSystem.CapacityToEvaFlow(result.proofData.capacity.paramValue.ToPares(), result.metricInch));
                    result.proofData.condflow.paramValue = DataFormat.EffNumber(MeasurementSystem.CapacityToCondFlow(result.proofData.capacity.paramValue.ToPares(), result.metricInch));

                    needCalculate = true;
                }
                var inputdata = _mapper.Map<ProjectInputDto>(result); 
                if (metricInch != result.metricInch) //加载默认参数或者存的参数  与系统设置的公英制不一致需转换
                {
                    _centrifugeCalculateHelper.UnitConvert(result.metricInch,metricInch, inputdata);
                    result.metricInch = metricInch;
                    needCalculate = true;
                }
                if (needCalculate)
                {
                    _centrifugeCalculateHelper.CalculateInput(inputdata);
                }
                //inputdata.projectVersion = "test";    //projectVersion 值类型 不会影响原始数据
                //result.proofData.capacity.paramValue = "1200";  //proofData 引用类型 影响原始数据
                return Ok(new ApiResult { data = inputdata });  //result
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 查询项目的所有版本
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProjectAllVersionById(string Id)
        {
            try
            {
                List<string> version = new List<string>();
                var result = await _projectServices.GetProjectsByProjectId(Id);
                foreach (var item in result)
                {
                    version.Add(item.projectVersion);
                }
                return Ok(new ApiResult { data = version });
            }
            catch (Exception ex)
            {

                return BadRequest(ex);
            }
           
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProjInfo(string proId,UpdateProjectbaseInfoDto updateProjectDto)
        {
            try
            {
                var proj = _projectbaseInfoServices.FindByExpress(p => p.ID == proId).FirstOrDefault();
                if (proj != null)
                {
                    _mapper.Map(updateProjectDto, proj);
                    _projectbaseInfoServices.Update(proj);
                }
                return Ok(new ApiResult());
            }
            catch (Exception EX)
            {

                return BadRequest(EX.Message);
            }
          
        }


        [HttpGet]
        public async Task<IActionResult> GetProjectsData([FromQuery]ProjectQueryParameters projectQueryParameters)
        {
            try
            {
                projectQueryParameters.OrderBy = "createTime desc";
                var result = await _projectServices.GetProjects(projectQueryParameters);
               
                return Ok(new ApiResult { data = _mapper.Map<IEnumerable<ProjectInfoDto>>(result) });  //_mapper.Map<IEnumerable<ProjectInfoDto>>(result)
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> GetLoactionInfo([FromQuery]CityQueryParameters cityQueryParameters)
        {
            CityResponse cityResponse = new CityResponse();
            var country_list = _cityServices.GetAllCountry();  //查询所有国家
            cityResponse.country = _mapper.Map<IEnumerable<CityResponseDto>>(country_list);

            if (cityQueryParameters.country != 0)
            {
                var province_list = _cityServices.FindByExpress(c => c.level == "2" && c.parent_id == cityQueryParameters.country.ToString()).ToList();//获取指定国家所有省会
                cityResponse.province = _mapper.Map<IEnumerable<CityResponseDto>>(province_list);


                if (cityQueryParameters.province != 0)
                {
                    var city_list = _cityServices.FindByExpress(c => c.parent_id == cityQueryParameters.province.ToString() && c.level == "3").ToList();
                    cityResponse.city = _mapper.Map<IEnumerable<CityResponseDto>>(city_list);
                }
            }
           

            return Ok(new ApiResult { data = cityResponse });
        }

        //[HttpGet]
        //public IActionResult Test()
        //{
           
        //    return Ok(_fakeData.InitInputData());
        //}

        [HttpGet]
        public IActionResult GetTime()
        {
            var vv = AppSettingsHelper.GetContent("MongoDbSetting", "ConnectionString");
            return Ok(new ApiResult { data = DateTime.Now.ToString() });
        }
    }
}
