$Server::BalancedMode = $BalanceMode::Option;

function zAdmin::SavePrefs()
{
	//export("zadmin::pref::*", "config\\zadmin.prefs.cs", False);
}

function zAdmin::LoadPrefs()
{
	if (isfile("config\\zadmin.prefs.cs"))
		exec("zadmin.prefs.cs");
}

function zAdmin::InitPrefs()
{
	zAdmin::LoadPrefs();
	
	$zadmin::pref::pickup::enabled = "true";
	
	//manually check each pref
	if ($zadmin::pref::log::KickBan == "") 
		$zadmin::pref::log::KickBan = "true";
	
	if ($zadmin::pref::log::Adminships == "") 
		$zadmin::pref::log::Adminships = "true";
	
	if ($zadmin::pref::log::MissionChanges == "") 
		$zadmin::pref::log::MissionChanges = "true";
	
	if ($zadmin::pref::log::PasswordChanges == "") 
		$zadmin::pref::log::PasswordChanges = "true";
	
	if ($zadmin::pref::log::Takeovers == "") 
		$zadmin::pref::log::Takeovers = "false";
	
	if ($zadmin::pref::log::Warnings == "") 
		$zadmin::pref::log::Warnings = "false";
	
	if ($zadmin::pref::log::AdminLogins == "") 
		$zadmin::pref::log::AdminLogins = "true";
	
	if ($zadmin::pref::log::AdminStrip == "") 
		$zadmin::pref::log::AdminStrip = "false";
	
	if ($zadmin::pref::log::TimeChanges == "") 
		$zadmin::pref::log::TimeChanges = "false";
	
	if ($zadmin::pref::log::TeamDamage == "") 
		$zadmin::pref::log::TeamDamage = "false";
	
	if ($zadmin::pref::log::GameModeChanges == "") 
		$zadmin::pref::log::GameModeChanges = "true";
	
	if ($zadmin::pref::log::Overflows == "") 
		$zadmin::pref::log::Overflows = "true";
	
	if ($zadmin::pref::log::Mute == "") 
		$zadmin::pref::log::Mute = "false";

	if ($zadmin::pref::log::Pickups == "")
		$zadmin::pref::log::Pickups = "false";
	
	if ($zadmin::pref::msg::friendlytakeover == "")
		$zadmin::pref::msg::friendlytakeover = "This server is reserved for a match right now.  Please drop from the server. Thanks.";

	if ($zadmin::pref::msg::firmtakeover == "")
		$zadmin::pref::msg::firmtakeover = "This server is reserved for a match at this time.  Leave or be kicked. Thanks.";

	if ($zadmin::pref::msg::permanentban == "")
		$zadmin::pref::msg::permanentban = "You have been PERMANENTLY banned from this server.  Come to #heaven on Dynamix IRC if you feel this message is in error.";

	if ($zadmin::pref::msg::globalspam == "")
		$zadmin::pref::msg::globalspam = "Your global message rights have been revoked due to excessive/abusive/childish chat.";

	if ($zadmin::pref::time::Kick == "")
		$zadmin::pref::time::Kick = "180";

	if ($zadmin::pref::time::Ban == "")
		$zadmin::pref::time::Ban = "1800";

	if ($zadmin::pref::server::TimeOffset == "")
		$zadmin::pref::server::TimeOffset = "0";

	if ($zadmin::pref::overflow::defaultpw == "")
		$zadmin::pref::overflow::defaultpw = " ";
	
	if ($zadmin::pref::overflow::secondpw == "")
		$zadmin::pref::overflow::secondpw = " ";

	if ($zadmin::pref::overflow::secondlevel == "")
		$zadmin::pref::overflow::secondlevel = " ";
	
	if ($zadmin::pref::overflow::overflowpw == "")
		$zadmin::pref::overflow::overflowpw = " ";

	if ($zadmin::pref::overflow::maxspots == "")
		$zadmin::pref::overflow::maxspots = "2";

	if ($zadmin::pref::overflow::forceEnterSAD == "")
		$zadmin::pref::overflow::forceEnterSAD = "true";

	if ($zadmin::pref::overflow::EnterSADTime == "")
		$zadmin::pref::overflow::EnterSADTime = "60";

	if ($zadmin::pref::server::playermax == "")
		$zadmin::pref::server::playermax = "26";

	if ($zadmin::pref::timestamper == "")
		$zadmin::pref::timestamper = "Patched";

	if ($zadmin::pref::afk::enabled == "")
		$zadmin::pref::afk::enabled = "true";

	if ($zadmin::pref::afk::monitorInterval == "")
		$zadmin::pref::afk::monitorInterval = "150";

	if ($zadmin::pref::afk::timelimit == "")
		$zadmin::pref::afk::timelimit = "300";
	
	if ($zadmin::pref::hm2::enabled == "")
		$zadmin::pref::hm2::enabled = "true";

	if ($zadmin::pref::antirape::enabled == "")
		$zadmin::pref::antirape::enabled = "true";

	if ($zadmin::pref::antirape::minteamsize == "")
		$zadmin::pref::antirape::minteamsize = "8";
	
	if ($zadmin::pref::antirape::norepair == "")
		$zadmin::pref::antirape::norepair = "true";

	if ($zadmin::pref::warnings::msg[1] == "")
	{
		$zadmin::pref::warnings::msg[1] = "TeamKill Warning";
		$zadmin::pref::warnings::text[1] = "Admin Warning: You are in danger of being kicked for teamkilling.";
	}

	if ($zadmin::pref::warnings::msg[2] == "")
	{
		$zadmin::pref::warnings::msg[2] = "Moron Warning";
		$zadmin::pref::warnings::text[2] = "Admin Warning: You are in danger of being kicked for being a moron.";
	}

	if ($zadmin::pref::warnings::msg[3] == "")
	{
		$zadmin::pref::warnings::msg[3] = " ";
		$zadmin::pref::warnings::text[3] = " ";
	}

	if ($zadmin::pref::warnings::msg[4] == "")
	{
		$zadmin::pref::warnings::msg[4] = " ";
		$zadmin::pref::warnings::text[4] = " ";
	}

	if ($zadmin::pref::warnings::msg[5] == "")
	{
		$zadmin::pref::warnings::msg[5] = " ";
		$zadmin::pref::warnings::text[5] = " ";
	}

	if ($zadmin::pref::warnings::msg[6] == "")
	{
		$zadmin::pref::warnings::msg[6] = " ";
		$zadmin::pref::warnings::text[6] = " ";
	}

	zAdmin::savePrefs();
	
	if ($zadmin::pref::overflow::defaultpw == " ")
		$zadmin::pref::overflow::defaultpw = "";
	if ($zadmin::pref::overflow::secondlevel == " ")
		$zadmin::pref::overflow::secondlevel = "";
	if ($zadmin::pref::overflow::secondpw == " ")
		$zadmin::pref::overflow::secondpw = "";
	if ($zadmin::pref::overflow::overflowpw == " ")
		$zadmin::pref::overflow::overflowpw = "";

	for (%i=1; %i<=6; %i++)
	{
		if ($zadmin::pref::warnings::msg[%i] == " ")
		{
			$zadmin::pref::warnings::msg[%i] = "";
			$zadmin::pref::warnings::text[%i] = "";
		}
	}
}