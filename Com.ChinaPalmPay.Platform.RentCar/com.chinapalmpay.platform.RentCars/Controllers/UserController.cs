﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Com.ChinaPalmPay.Platform.RentCar.Model;
using Com.ChinaPalmPay.Platform.RentCar.Model.OperationResult;
using Com.ChinaPalmPay.Platform.RentCar.IBLLS;
using Com.ChinaPalmPay.Platform.RentCar.BLLFacs;
using System.IO;
using Com.ChinaPalmPay.Platform.RentCar.Model.ParamModel;
using Com.Chinapalmpay.Component.Log;
using Com.ChinaPalmPay.Platform.RentCar.Common;
using Com.ChinaPalmPay.Platform.RentCar.SQLServer;
using Com.ChinaPalmPay.Platform.RentCar.DALFactory;


namespace com.chinapalmpay.platform.RentCars.Controllers
{
    public class UserController : BaseController
    {
        //消息查询接口
        private static readonly UserOperations db_manager = DataAccess.CreateUserDbManager();
        public UserController()
        {

        }
        //
        // GET: /uSER/
        private static readonly IUserHandler userbll = BllAccess.CreateUserService();
        public ActionResult queryMsg()
        {
            return View();
        }
        //根据用户id查询消息

        [HttpPost]
        public ActionResult queryMsg(string UserId)
        {
            DefaultResult result = new DefaultResult();
            try
            {
                if (!String.IsNullOrWhiteSpace(UserId))
                {
                    IList<Messages> m = userbll.queryMessageHandler(UserId);
                    if (m != null && m.Count() > 0)
                    {
                        result.Code = "0000";
                        result.Data = m;
                        result.Message = "查询消息成功";
                        LogHelper.OutPut(this.Url.RequestContext, m);

                    }
                    else
                    {
                        result.Code = "0001";
                        result.Data = m;
                        result.Message = "查询消息失败";
                        LogHelper.OutPut(this.Url.RequestContext, "查询消息失败");
                    }

                }
                else
                {
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "查询消息参数有空值";

                }
            }
            catch(Exception e)
            {
                result.Code = "0201";
                result.Data = "";
                result.Message ="系统繁忙,请联系客服";
                LogHelper.Exception(this.Url.RequestContext,e); ;
            }
            return Json(result);
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(UserReg infos, HttpPostedFileBase UserDriverLicense)
        {
            LogerHelper.debug("/User/Register" +infos.LoginID+"  "+infos.LoginPwd);
            DefaultResult result = new DefaultResult();
            try
            {
                //判断是普通用户
                if (String.IsNullOrWhiteSpace(infos.LoginID) || String.IsNullOrWhiteSpace(infos.LoginPwd)
                    || String.IsNullOrWhiteSpace(infos.CardId)
                    || UserDriverLicense == null
                    || String.IsNullOrWhiteSpace(infos.appkey) || String.IsNullOrWhiteSpace(infos.phone) ||
                    String.IsNullOrWhiteSpace(infos.zone) || String.IsNullOrWhiteSpace(infos.code)
                    )
                {
                    //注册信息不全,返回json字符串{‘’：‘’}
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "用户注册输入参数有空值";
                    //账号：11位数字
                    //密码：以字母开头，长度在6~18之间，只能包含字符、数字和下划线。
                }
                //else if (!(Regex.IsMatch(info.Telephone,@"^[0-9]{11}$")&&Regex.IsMatch(info.Password,@"^[0-9a-zA-Z]{32}$")))
                //{
                //    json.Data = new UserRegResult() { Code = "00", Response = "00", Data = "00", Message = "输入参数格式不对" };
                //}
                else
                {
                    string DriverLicensePath = string.Empty;
                    DriverLicensePath = SaveFile(UserDriverLicense, DriverLicensePath);
                    infos.UserDriveLicense = DriverLicensePath;
                    //创建普通用户并增加权限
                    UserLogin logins = userbll.UserRegHandler(infos);
                    if (logins != null)
                    {//账号不存在
                        //USER_MANAGER.addPrivilege(current);
                        result.Code = "0000";
                        result.Data = "";
                        result.Message = "用户注册成功";
                        LogHelper.OutPut(this.Url.RequestContext, "用户注册成功");
                    }
                    else
                    {//账号存在
                        result.Code = "0001";
                        result.Data = "";
                        result.Message = "用户注册失败";
                        LogHelper.OutPut(this.Url.RequestContext, "用户注册失败");
                    }
                }
            }
            catch (Exception e)
            {
                result.Code = "0201";
                result.Data = "";
                result.Message ="系统繁忙,请联系客服";
                 LogHelper.Exception(this.Url.RequestContext,e);
                //注册出现异常
            }
            return Json(result);
        }
        //查询微信账号是否存在
        [HttpPost]
        public ActionResult verifyOpenId(string openid)
        {
            DefaultResult result = new DefaultResult();
            try
            {
                if (!String.IsNullOrWhiteSpace(openid))
                {
                    User user = userbll.queryOpenIdHandler(openid);
                    if (user != null)
                    {
                        LogerHelper.debug("查询openid"+openid);
                        result.Code = "0000";
                        result.Data = new { UserId = user.UserId, PhoneNumber=user.PhoneNumber,Password=user.Password,CardID = user.CardID, UserDriverLicense = Properties.getHost() + user.UserDriverLicense, UserStatus = user.UserStatus };
                        result.Message = "微信账号openId已存在";
                        LogHelper.OutPut(this.Url.RequestContext, new { UserId = user.UserId, PhoneNumber = user.PhoneNumber, Password = user.Password, CardID = user.CardID, UserDriverLicense = Properties.getHost() + user.UserDriverLicense, UserStatus = user.UserStatus });
                    }
                    else
                    {
                        result.Code = "0001";
                        result.Data = null;
                        result.Message = "微信账号openId不存在";
                        LogHelper.OutPut(this.Url.RequestContext, "微信账号openId不存在");
                    }
                }
                else
                {
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "微信账号openid参数为null";
                }

            }
            catch (Exception e)
            {
                result.Code = "0201";
                result.Data = "";
                result.Message = "系统繁忙,请联系客服";
                LogHelper.Exception(this.Url.RequestContext,e);
            }
            return Json(result);
        }
        //微信用户绑定
        [HttpGet]
        public ActionResult WeChatUserReg()
        {
            return View();
        }
        [HttpPost]
        public ActionResult WeChatUserReg(string PhoneNumber,string Password,string IDCardNo, string OpenID, HttpPostedFileBase UserDriverLicense)
        {
            LogerHelper.debug("/User/WeChatUserReg  " + PhoneNumber+"  " + Password+"  " +IDCardNo + "  " + OpenID + "  " + UserDriverLicense);
            DefaultResult result = new DefaultResult();
            try
            {
                if (!String.IsNullOrWhiteSpace(PhoneNumber) && !String.IsNullOrWhiteSpace(Password)&&!String.IsNullOrWhiteSpace(OpenID) && !String.IsNullOrWhiteSpace(IDCardNo) && UserDriverLicense != null)
                {

                    User userByCardID = db_manager.getUserByCardID(IDCardNo);
                    

                    //if (userByCardID!=null && userByCardID.PhoneNumber == PhoneNumber) {
                    //    result.Code = "0005";
                    //    result.Data = null;
                    //    result.Message = "身份证已存在，手机号码不一致";
                    //    LogHelper.OutPut(this.Url.RequestContext, "微信用户绑定失败");
                    //    return Json(result);
                    //}

                    //User userByPhoneNumber = db_manager.getUserByPhoneNumber(PhoneNumber);
                    //if (userByPhoneNumber != null && userByPhoneNumber.CardID == IDCardNo)
                    //{
                    //    result.Code = "0006";
                    //    result.Data = null;
                    //    result.Message = "手机号码已存在，身份证不一致";
                    //    LogHelper.OutPut(this.Url.RequestContext, "微信用户绑定失败");
                    //    return Json(result);
                    //}
                    if (userByCardID != null && !userByCardID.PhoneNumber.Equals(PhoneNumber))
                    {
                        result.Code = "0005";
                        result.Data = null;
                        result.Message = "身份证已存在，手机号码不一致";
                        LogHelper.OutPut(this.Url.RequestContext, "微信用户绑定失败");
                        return Json(result);
                    }

                    User userByPhoneNumber = db_manager.getUserByPhoneNumber(PhoneNumber);
                    if (userByPhoneNumber != null && !userByPhoneNumber.CardID.Equals(IDCardNo))
                    {
                        result.Code = "0006";
                        result.Data = null;
                        result.Message = "手机号码已存在，身份证不一致";
                        LogHelper.OutPut(this.Url.RequestContext, "微信用户绑定失败");
                        return Json(result);
                    }



                    string DriverLicensePath = string.Empty;
                    // filePicturePath = SaveFile(filePicture, filePicturePath);
                    // info.PitcurePath = filePicturePath;
                    DriverLicensePath = SaveFile(UserDriverLicense, DriverLicensePath);
                    User use = userbll.WechatUserReg(PhoneNumber, Password,IDCardNo, OpenID, DriverLicensePath);
                    if (use != null)
                    {
                        result.Code = "0000";
                        result.Data = new { userId = use.UserId, CardId = use.CardID, UserDriverLicense = Properties.getHost() + use.UserDriverLicense, UserStatus = use.UserStatus };
                        result.Message = "微信用户绑定成功";
                        LogHelper.OutPut(this.Url.RequestContext,new { userId = use.UserId, CardId = use.CardID, UserDriverLicense = Properties.getHost() + use.UserDriverLicense, UserStatus = use.UserStatus });
                    }
                    else
                    {
                        result.Code = "0001";
                        result.Data = null;
                        result.Message = "微信用户绑定失败";
                        LogHelper.OutPut(this.Url.RequestContext,"微信用户绑定失败");
                    }
                }
                else
                {
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "微信用户绑定输入参数有空值";

                }
            }
            catch (Exception ex)
            {
                LogerHelper.debug("绑定异常!" + ex.Message);
                result.Code = "0201";
                result.Data = "";
                result.Message = "系统繁忙,请联系客服";
                LogHelper.Exception(this.Url.RequestContext, ex);
            }
            return Json(result);
        }
        public ActionResult Login(string Returnurl)
        {
            ViewBag.returnurl = Returnurl;
            return View();
        }
        [HttpPost]
        public ActionResult Login(UserReg info, string Returnurl)
        {
            DefaultResult result = new DefaultResult();
            try
            {//信息缺少
                if (String.IsNullOrWhiteSpace(info.LoginID) || String.IsNullOrWhiteSpace(info.LoginPwd))
                {
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "用户登陆输入参数有空值";
                }
                //else if (!(Regex.IsMatch(info.Telephone, @"^[0-9]{11}$") && Regex.IsMatch(info.Password, @"^[0-9a-zA-Z]{32}$")))
                //{
                //    json.Data = new UserRegResult() { Code = "00", Response = "00", Data = "00", Message = "输入参数格式不对" };
                //}
                else
                {//格式正确
                    string userId = userbll.UserLoginHandler(info);
                    if (userId != null)
                    {
                        //登陆成功
                        result.Code = "0000";
                        result.Data = userId;
                        result.Message = "用户登陆成功";
                        LogHelper.OutPut(this.Url.RequestContext, userId);
                    }
                    else
                    {
                        result.Code = "0001";
                        result.Data = "";
                        result.Message = "用户账号或密码错误";
                        LogHelper.OutPut(this.Url.RequestContext, "用户账号或密码错误");
                    }
                }
            }
            catch (Exception e)
            {
                result.Code = "0201";
                result.Data = "";
                result.Message = "系统繁忙,请联系客服";
                LogHelper.Exception(this.Url.RequestContext,e);
                //登陆出现异常
            }
            return Json(result);
        }
        //修改用户密码
        public ActionResult UpdateUser()
        {
            return View();
        }
        //修改密码
        [HttpPost]
        public ActionResult UpdateUser(ChangePwd login)
        {
            DefaultResult result = new DefaultResult();
            try
            {//空值
                if (!String.IsNullOrWhiteSpace(login.OldPwd) && !String.IsNullOrWhiteSpace(login.LoginPwd) && !String.IsNullOrWhiteSpace(login.UserID))
                {
                    UserLogin logins = userbll.UserUpdateHandler(login.OldPwd, login.LoginPwd, login.UserID);
                    if (logins != null)
                    {
                        result.Code = "0000";
                        result.Data = logins;
                        result.Message = "修改成功,请重新登录";
                        LogHelper.OutPut(this.Url.RequestContext, logins);
                    }
                    else
                    {
                        result.Code = "0001";
                        result.Data = "";
                        result.Message = "修改密码失败";
                        LogHelper.OutPut(this.Url.RequestContext, "修改密码失败");
                    }

                }
                else
                {
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "修改密码参数有空值";

                }
            }
            catch (Exception e)
            {
                result.Code = "0201";
                result.Data = "";
                result.Message = "系统繁忙,请联系客服";
                LogHelper.Exception(this.Url.RequestContext,e);

            }
            return Json(result);
        }
        //完善个人资料
        public ActionResult Complete()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Complete(User info, HttpPostedFileBase fileDriverLicense)
        {
            DefaultResult result = new DefaultResult();
            try
            {//驾驶证号 姓名 userid不为空
                if (!String.IsNullOrWhiteSpace(info.Name) && !String.IsNullOrWhiteSpace(info.CardID) && !String.IsNullOrWhiteSpace(info.UserId) && fileDriverLicense != null)
                {
                    string filePicturePath = string.Empty, DriverLicensePath = string.Empty;
                    // filePicturePath = SaveFile(filePicture, filePicturePath);
                    // info.PitcurePath = filePicturePath;
                    DriverLicensePath = SaveFile(fileDriverLicense, DriverLicensePath);
                    info.UserDriverLicense = DriverLicensePath;
                    User user = userbll.UserCompleteHandler(info);
                    if (user != null)
                    {
                        result.Code = "0000";
                        result.Data = user;
                        result.Message = "用户资料完善成功";
                        LogHelper.OutPut(this.Url.RequestContext, user);
                    }
                    else
                    {
                        result.Code = "0001";
                        result.Data = user;
                        result.Message = "用户资料完善失败";
                        LogHelper.OutPut(this.Url.RequestContext, "用户资料完善失败");
                    }
                }
                else
                {
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "用户资料完善参数有空值";

                }
            }
            catch (Exception ex)
            {
                result.Code = "0201";
                result.Data = "";
                result.Message = "系统繁忙,请联系客服";
                LogHelper.Exception(this.Url.RequestContext, ex);
            }
            return Json(result);
        }
        private string SaveFile(HttpPostedFileBase file, string path)
        {
            try {
                if (file != null)
                {
                    string legalCard = Guid.NewGuid().ToString().Replace("-", "") + "_" + Path.GetFileName(file.FileName);
                    //string paths=AppDomain.CurrentDomain.BaseDirectory;
                    path = "/upload/images/" + legalCard;
                    file.SaveAs(Server.MapPath("~" + path));
                }
            }
            catch(Exception ex)
            {
                LogerHelper.exception("图片上传失败！",ex);

            }
            return path;
        }
        [NonAction]
        public ActionResult RedirectToLocal(string ReturnUrl)
        {
            if (Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        //修改手机号码
        public ActionResult ChangeTel()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangeTel(ChangeTel tel)
        {
            DefaultResult result = new DefaultResult();
            try
            {
                if (!String.IsNullOrWhiteSpace(tel.LoginID) && !String.IsNullOrWhiteSpace(tel.UserID))
                {
                    UserLogin user = userbll.changeTelHandler(tel);
                    if (user != null)
                    {
                        result.Code = "0000";
                        result.Data = user;
                        result.Message = "修改成功,请重新登录";
                        LogHelper.OutPut(this.Url.RequestContext, user);
                    }
                    else
                    {
                        result.Code = "0001";
                        result.Data = "";
                        result.Message = "用户手机号修改失败";
                        LogHelper.OutPut(this.Url.RequestContext, "用户手机号修改失败");
                    }
                }
                else
                {
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "用户手机号修改操作参数有空值";


                }
            }
            catch (Exception e)
            {
                result.Code = "0201";
                result.Data = "";
                result.Message = "系统繁忙,请联系客服";
                LogHelper.Exception(this.Url.RequestContext,e);
            }
            return Json(result);
        }
        //用户资料查询接口
        public ActionResult UserInfomation()
        {
            return View();
        }
        //查询用户个人资料
        [HttpPost]
        public ActionResult UserInfomation(User user)
        {
            DefaultResult result = new DefaultResult();
            try
            {//用户id有值
                if (!String.IsNullOrWhiteSpace(user.UserId))
                {
                    User USER = userbll.findUserInfomation(user.UserId);
                    if (USER != null)
                    {
                        result.Code = "0000";
                        result.Data = USER;
                        result.Message = "查询用户资料成功";
                        LogHelper.OutPut(this.Url.RequestContext, USER);
                    }
                    else
                    {
                        result.Code = "0001";
                        result.Data = "";
                        result.Message = "查询用户资料失败";
                        LogHelper.OutPut(this.Url.RequestContext, "查询用户资料失败");
                    }
                }
                else
                {
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "查询用户资料操作参数有空值";


                }
            }
            catch (Exception e)
            {
                result.Code = "0201";
                result.Data = "";
                result.Message = "系统繁忙,请联系客服";
                LogHelper.Exception(this.Url.RequestContext,e);
            }
            return Json(result);
        }
        public ActionResult ChangePhoto()
        {
            return View();
        }
        //修改头像接口
        [HttpPost]
        public ActionResult ChangePhoto(string UserID, HttpPostedFileBase filePhoto)
        {
            DefaultResult result = new DefaultResult();
            try
            {
                if (!String.IsNullOrWhiteSpace(UserID) && filePhoto != null)
                {
                    string filePhotoPath = string.Empty;
                    filePhotoPath = SaveFile(filePhoto, filePhotoPath);
                    User user = userbll.changePhoto(UserID, filePhotoPath);
                    if (user != null)
                    {
                        result.Code = "0000";
                        result.Data = user;
                        result.Message = "用户头像修改成功";
                        LogHelper.OutPut(this.Url.RequestContext, user);
                    }
                    else
                    {
                        result.Code = "0001";
                        result.Data = user;
                        result.Message = "用户头像修改失败";
                        LogHelper.OutPut(this.Url.RequestContext, "用户头像修改失败");
                    }
                }
                else
                {
                    result.Code = "0101";
                    result.Data = "";
                    result.Message = "用户头像修改参数有空值";

                }
            }
            catch (Exception ex)
            {
                result.Code = "0201";
                result.Data = "";
                result.Message = "系统繁忙,请联系客服";
                LogHelper.Exception(this.Url.RequestContext, ex);
            }
            return Json(result);

        }
    }
}
