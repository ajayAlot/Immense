using System.ComponentModel.DataAnnotations.Schema;

public class UsersPanelSettings
{
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("website_address")]
    public string WebsiteAddress { get; set; }

    [Column("company_name")]
    public string CompanyName { get; set; }

    [Column("company_tagline")]
    public string CompanyTagline { get; set; }

    [Column("company_logo")]
    public string CompanyLogo { get; set; }

    [Column("support_url")]
    public string SupportUrl { get; set; }

    [Column("support_mobile")]
    public string SupportMobile { get; set; }

    [Column("support_email")]
    public string SupportEmail { get; set; }

    [Column("logout_url")]
    public string LogoutUrl { get; set; }

    [Column("theme")]
    public string Theme { get; set; }

    [Column("front_theme")]
    public string FrontTheme { get; set; }

    [Column("background_image")]
    public string BackgroundImage { get; set; }
}