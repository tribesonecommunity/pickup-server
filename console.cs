//-----------------------------------
//
// Tribes startup
//
//-----------------------------------

$Game::Midair::Height = 4;
$Game::LT::Countdown = true;
$Game::LT::MidAirBeep = "~whit.wav";


$zadmin::version = "0.935";
$zadmin::state = "LT Stripped v1.5 + Anti-Scum";

$ModInfo = "zadmin <f2>v"@$zadmin::version@"<f1>/<f2>"@$zadmin::state;


$dedicated = true;
$WinConsoleEnabled = $dedicated;
$Console::logBufferEnabled = !$dedicated; // turn off window scroll back
$Console::logmode=1;
$Console::Prompt = "% ";

newServer();
focusServer();

function EvalSearchPath()
{
   // search path always contains the config directory
   %searchPath = "config";
   if($modList == "")
      $modList = "base";
   else
   {
      for(%i = 0; (%word = getWord($modList, %i)) != -1; %i++)
         if(%word == "base")
            break;
      if(%word == -1)
         $modList = $modList @ " base";
   }
   for(%i = 0; (%word = getWord($modList, %i)) != -1; %i++)
   {
      %addPath = %word @ ";" @ %word @ "\\missions;" @ %word @
         "\\fonts;" @ %word @ "\\skins;" @ %word @ "\\voices;" @ %word @ "\\scripts\\";
      %searchPath = %searchPath @ ";" @ %addPath;
   }
   %searchPath = %searchPath @ ";recordings;temp;plugins\\scripts";
   echo(%searchPath);

   $ConsoleWorld::DefaultSearchPath = %searchPath;

   // clear out the volumes:
   for(%i = 0; isObject(%vol = "VoiceVolume" @ %i); %i++)
      deleteObject(%vol);
   for(%i = 0; isObject(%vol = "SkinVolume" @ %i); %i++)
      deleteObject(%vol);

   // load all the volumes:
   %file = File::findFirst("voices\\*.vol");
   for(%i = 0; %file != ""; %file = File::findNext("voices\\*.vol"))
      if(newObject("VoiceVolume" @ %i, SimVolume, %file))
         %i++;

   %file = File::findFirst("skins\\*.vol");
   for(%i = 0; %file != ""; %file = File::findNext("skins\\*.vol"))
      if(newObject("SkinVolume" @ %i, SimVolume, %file))
         %i++;
}

//
EvalSearchPath();
//newObject(ScriptsVolume, SimVolume, "scripts.vol");
newObject(EntitiesVolume, SimVolume, "entities.vol");
//newObject(dmtScripts, SimVolume, "dmt.vol");
//newObject(InterfaceVolume, SimVolume, "interface.vol");

//start her up
exec("attachment");
exec("CrashProtectionPack.cs");
exec("code.missionList.cs");

// Load prefs and execute any autoexec commands...
exec("pref.serverdefaults.cs");
exec("serverprefs.cs");
exec("code.server.cs");
exec("code.game.cs");
exec("code.observer.cs");
exec("code.player.cs");
exec("autoexec.cs");
exec("code.lasthope.cs");
exec("code.balancedmode.cs");
exec("freeze.cs");
exec("code.overtime.cs");
exec("code.notifications.cs");
exec("code.midair.cs");
exec("code.midair.disc.cs");
exec("code.midair.nadejump.cs");
exec("code.smurfscanner.cs");
exec("code.antiscum.cs");

createServer($HostMission, True);
translateMasters();

echo("Dedicated Server Initialized");
