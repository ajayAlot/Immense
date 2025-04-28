using Microsoft.AspNetCore.Mvc;
using processJobAndSmsApi.Models;
using processJobAndSmsApi.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using processJobAndSmsApi.Services;
using System.Text;
using Newtonsoft.Json;

namespace ProcessJobAndSmsApi.Controllers
{
    
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly HelperService _helperService;

        public AccountController(ILogger<AccountController> logger,ApplicationDbContext context, HelperService helperService)
        {
            _logger = logger;
            _context = context;
            _helperService = helperService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var username = _helperService.GetLoggedUsername();
            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginModel model){
            if(ModelState.IsValid){
                var user =await _context.Users.FirstOrDefaultAsync(u=>u.Username==model.username);
                if(user!=null && BCrypt.Net.BCrypt.Verify(model.password,user.Password)){
                    HttpContext.Session.SetString("username",model.username);
                    var usercategory = await _context.UsersCategory.FirstOrDefaultAsync(u => u.Id.ToString() == user.CategoryId);
                    var json = JsonConvert.SerializeObject(usercategory);
                    HttpContext.Session.SetString("usercategory", json);
                    return Json(StatusCode(200));
                }
                return Json(new {success=false, message="User Not Found"});
            }
            return Json(new {success=false, message="The data is not in proper model format"});
        }

        [HttpGet]
        public IActionResult Register(){
            var username = _helperService.GetLoggedUsername();
            if(!string.IsNullOrEmpty(username)) return RedirectToAction("Index","Home");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody] UserRegister model)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                    if (user != null)
                    {
                        return Json(new { success = false, message = "Username already exists" });
                    }
                    else
                    {

                        var randompassword = _helperService.PasswordGenerate();
                        var newuser = new Users
                        {
                            Username = model.Username,
                            CategoryId = _helperService.GetUsersCategoryIDByTitle("Customer"),
                            TimezoneId = "254",
                            Email = model.Email,
                            Mobile = model.Mobile,
                            Fullname = model.Fullname,
                            TotalBalance = "0.00",
                            Status = "Inactive",
                            Approve = "Yes",
                            RegistrationDate = DateTime.Now.ToString("d MMM yyyy, h:mm tt").ToLower(),
                            TotalLogin = "0",
                            LastLoginDate = "",
                            Password = BCrypt.Net.BCrypt.HashPassword(randompassword),
                            Zipcode = model.PostCode,
                            ApiKey = _helperService.GenerateApiKey(model.Username),
                            LastAccountUpdate = DateTime.Now.ToString("d MMM yyyy, h:mm tt").ToLower(),
                            LastLoginJdate = "",
                            Pdate = DateOnly.FromDateTime(DateTime.Now).ToString(),
                            Jdate = _helperService.DateToJulian(DateTime.Now),
                            IsOnline = "No",
                            Timestamp = TimeOnly.FromDateTime(DateTime.Now).ToString(),
                            BillType = "Prepaid",
                            ParentId = "1",
                            ManagerId = _helperService.GetRandomManagerIDByUsersID(_helperService.GetLoggedUsername()),
                            IndustryId = "1",
                            CreatedById = "1",
                            MdEmail = _helperService.ToMD5(model.Email),
                            Firstname = "",
                            Lastname = "",
                            Gender = "",
                            Address = "",
                            State = "",
                            City = "",
                            Verified = "",
                            RegistrationJdate = _helperService.DateToJulian(DateTime.Now),
                            LastPasswordUpdate = DateTime.Now.ToString("d MMM yyyy, h:mm tt").ToLower(),
                            VerificationCode = "",

                        };
                        await _context.Users.AddAsync(newuser);
                        await _context.SaveChangesAsync();

                        var panelsetting = new UsersPanelSettings
                        {
                            UserId = newuser.Id.ToString(),
                            WebsiteAddress="",
                            CompanyName=model.Companyname,
                            CompanyTagline="",
                            CompanyLogo= "",
                            SupportUrl="",
                            SupportMobile="",
                            SupportEmail="",
                            LogoutUrl="",
                            Theme="",
                            FrontTheme="",
                            BackgroundImage=""


                        };

                        await _context.UsersPanelSettings.AddAsync(panelsetting);
                        await _context.SaveChangesAsync();
                        string smslimit = "10";
                        var useraccess = new UserAccess
                        {
                            UserId = newuser.Id.ToString(),
                            TextSms = "Yes",
                            FlashSms = "Yes",
                            SmartSms = "No",
                            UnicodeSms = "Yes",
                            Campaign = "No",
                            Compose = "Yes",
                            DynamicSms = "No",
                            AllowSpam = "No",
                            RestrictedReport = "Yes",
                            NumberManagement = "No",
                            AllowSmpp = "No",
                            GatewayFamilyId = "Default",
                            RegionId = "No",
                            ApiAccess = "Yes",
                            ReportBlock = "No",
                            DndRefund = "No",
                            OnRefund = "No",
                            Template = "No",
                            RestrictedTemplate = "No",
                            DynamicSender = "No",
                            Overselling = "No",
                            ApplyTree = "No",
                            ApplyTreeCutting = "No",
                            SmsLimit = smslimit,
                            VerifiedSms = "No",
                            RcsMsg = "No" // Default value for enum field
                        };

                        await _context.UserAccess.AddAsync(useraccess);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();


                        return Json(new { success = true, message = "Registration successful", password = randompassword });




                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error registering user.");
                    return Json(new { success = false, message = ex.Message });
                }
                
            }

            return Json(new { success = false, message = "Invalid model data" });
        }

        [HttpGet]
        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

    }
}
