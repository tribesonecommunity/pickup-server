###########################################################################################
##
##         WORSTAIM'S CRASH PROTECTION PACK
##

// DESCRIPTION:
//
// This pack contains a patch to every vulnerability
// and exploit that exists in Starsiege: Tribes - Base Mod
// from the 1.11 patch and up that I am aware of.
// With all this code, make sure you cut and paste it to where I tell you.
// Then execute this file by adding the line: exec("CrashProtectionPack.cs"); 
// into your autoexec.cs file.


############################################################################################

/////////////////////////////////////////////////
// FUNCTIONS YOU NEED FOR THIS PACK TO WORK
////////////////////////////////////////////////

function String::containsCrash(%clientId, %message, %function)
{
   if(%clientId.isCrasher)
   {
	return true;
   }
   //\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\||
   //                                               ||
   // KingTomato's Crash Protection (Best There Is) ||
   //						    ||
   // MODIFIED BY WORSTAIM		            ||
   /////////////////////////////////////////////////||
   %count[Hex] = 0;
   %count[Tab] = 0;
   %count[Cr]  = 0;
   for (%a = 0; (%letter = String::GetSubStr(%message, %a, 1)) != ""; %a++)
   {
	if (%letter == "\t")
	{
		%count[Tab]++;
	}
	else if (%letter == "\n")
	{
		%count[Cr]++;
	}
	else if (String::findSubStr(escapeString(%letter), "\\x") == 0)
	{	
		%count[Hex]++;
	}
   }
   if(%count[Tab] + %count[Hex] + %count[Cr] >= 80 || String::len(%message) >= 1000)
   {
	%clientId.gag = true;
	%clientId.isCrasher = true;
	bottomprintall("<jc><F2>"@client::getName(%clientId)@" <F1>Tried To Crash The Server And Has Been <F0>PERMANENTLY BANNED", 6);
	Log::Exploit(%clientId, "Server-Crash", %function, %message);
	ban(%clientId, "Permanent", -1);
	return true;
   }
   return false;
}

//****************************************************************************
//ban("ClientID of Person Being Banned", "Type Of Ban", "ClientID of Banner");
//Bans and then kicks the client
//****************************************************************************
function ban(%cl, %banType, %admin)
{
	if(%cl != %admin.selClient && %admin != -1)
	{
		client::sendMessage(%admin, 1, "Go fuck yourself.");
		return;
	}
	%name = client::getName(%cl);
	%ip = Client::trimIP(%cl);
	%bannedBy = client::getName(%admin);

	banlist::remove(client::wPort(%cl)); // Seems Counter-Intuitive, But It's Not

	if(%admin == -1)
	{
		%bannedBy = "The Server";
	}

	if(%banType == "Permanent")
	{
		echo(%bannedBy@" has banned: "@%name@", "@%banType@"ly");
		messageall(1,%bannedBy@" has banned "@%name@" Permanently");
		banlist::addAbsolute(%ip, 0);
		kick(%cl, "You have been PERMANENTLY BANNED");
	}

	else if(%banType == "Temporary")
	{
		echo(%bannedBy@" has banned: "@%name@", Temporarily  (1 Week)");
		messageall(1,%bannedBy@" has banned "@%name@" Temporarily (1 Week)");
		banlist::add(%ip, 604800);
		kick(%cl, "You have been BANNED for 1 week");
	}
	banlist::export("config\\banlist.cs"); //Crash The Server And Your Still Banned
	echo("BAN: "@%bannedBy@" banned "@%name@" ("@%banType@")");
}

//***************************
// kick(%clientId, %message);
//***************************

function kick(%clientId, %msg)
{
	//I Use This Because Tribes Crashes For Some Reason When Net::Kick() Is Called
	//From Inside Any Function Except Admin::Kick() Without Being schedule()'d

	schedule("net::kick("@%clientId@", \""@%msg@"\");",0.2);
}


//*******************************
//Client::trimIP(clientId);
//*******************************
function Client::trimIP(%clientId)
{
   // Should Turn This: IP:#.#.#.#:# into IP:#.#.#.*
   // Would Turn IP:012.345.678.901:2345 Into IP:012.345.678.*
   %ipstr = client::getTransportAddress(%clientId);
   %ip = string::getSubStr(%ipstr, 0, 15);
   for(%i = 0; string::getSubStr(%ip, %i, 1) != ""; %i++)
   {
	%num = string::getSubStr(%ip, %i, 1);
	if(!String::ICompare(%num, "."))
	{
		%dot++;
		if(%dot == "3")
		{
			%finalIp = string::getSubStr(%ip, 0, %i++);
			%finalIP = %finalIP@"*";
			return %finalIP;
		}
	}
	
   }
   return %ip;
}

//***************************************************************
//Admin::Exploit(exploiterID, "ExploitType (info)", KickPlayer?);
//ex: Admin::Exploit(2049, "Admin Option (Ban)", true);
//***************************************************************

function Admin::Exploit(%e, %o, %k)
{
	%name = client::getName(%e);
	%ip = client::getTransportAddress(%e);
	
	$player = %name@" - "@%ip;
	$exploit = %o;
	$s = "----------------------------------------------------";
	export("$s", "config\\"@$ModName@"_Exploits.log", true);
	export("$player", "config\\"@$ModName@"_Exploits.log", true);
	export("$exploit", "config\\"@$ModName@"_Exploits.log", true);

	echo("ADMIN EXPLOIT: "@$player@" - "@%o);

	if(%k)
	{
		messageall(0, %name@" has been auto-kicked for attempting to use an exploit.");
		kick(%e, "You've Been Kicked, Go Away.");
	}
}

function Log::Exploit(%clientId, %type, %function, %var)
{
	%name = client::getName(%clientId);
	%ip = client::getTransportAddress(%clientId);
	
	$player = %name@" - "@%ip;
	$exploit = %type;
	$function = %function;
	if(%var != -1)
	{
		$var = %var;
	}
	$s = "---------------------------------------------------------------------";
	export("$s", "config\\"@$modName@"_Exploits.log", true);
	export("$player", "config\\"@$modName@"_Exploits.log", true);
	export("$exploit", "config\\"@$modName@"_Exploits.log", true);
	export("$function", "config\\"@$modName@"_Exploits.log", true);
	export("$var", "config\\"@$modName@"_Exploits.log", true);
}

function Client::wPort(%clientId)
{
   %ipstr = client::getTransportAddress(%clientId);
   %ip = string::getSubStr(%ipstr, 3, 16);
   for(%i = 0; string::getSubStr(%ip, %i, 1) != ""; %i++)
   {
	if(string::getSubStr(%ip, %i, 1) == ":")
	{
		return "IP:"@string::getSubStr(%ip, 0, %i)@":*";
	}
	
   }
}

$modname = $modList;



