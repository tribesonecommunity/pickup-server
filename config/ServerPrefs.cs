//////////////////////////////////////////
/// Server Settings
//////////////////////////////////////////
   $Server::HostName = "PU Server Killer Stork";
   $Server::MaxPlayers = "16";
   $Server::Password = "";
   
   $pref::LastMission = "DangerousCrossingLT";

   $Server::Info = "Join the Tribes discord https://playt1.com/discord\n\nPU Server Version - "@ $Server::VersionControl @" / Last Updated "@ $Server::LastUpdatedDate;
   $Server::JoinMOTD = "\n<jc>Join the Tribes discord https://playt1.com/discord\n";
//////////////////////////////////////////

//////////////////////////////////////////
/// IP Address & Networking
   $Server::HostPublicGame = "false";

   if ($Server::HostPublicGame) {
      // Change these values only.
      $Server::IP = "67.222.138.46";
      $Server::Port = "28004";
   }
   else {
      // This is your server IP to LOCALLY aka not online.
      // $Server::IP = "192.168.1.1";
      $Server::IP = "127.0.0.1";
      // $Server::Port = "28004";
      $Server::Port = "28001";
   }

/// Format the IP Address.
$Server::Address = "IP:"@ $Server::IP @":" @ $Server::Port;
//////////////////////////////////////////

//////////////////////////////////////////
//Basic pickup rule, balanced and tourney are true, and no overtime
$BalanceMode::Option = 2;
$Server::TourneyMode = false;
$Game::LT::OvertimeEnabled = false;
$Server::timeLimit = "30";
//////////////////////////////////////////

//////////////////////////////////////////
$Stats::Awards::enabled = true; // toggles awards showing on stats screen
$Lasthope::enabled = true;  // toggles lasthope
$Lasthope::strict = false;  // this will kick everything that's not 1.41, i don't recommend it
$HitSounds::enabled = true;
$QuakeAnnounce::enabled = true;
$mj::enableObsWarning = false;
$Server::AutoAssignTeams = "False";
$Server::FloodProtectionEnabled = "true";

$Server::CurrentMaster = "0";
$server::MasterAddressN0 = "t1m1.pu.net:28000 t1m1.tribes1.co:28000 t1m1.lock-load.org:28000 t1m2.lock-load.org:28000 t1m3.lock-load.org:28000";
$Server::MasterName0 = "unofficial";
$Server::XLMasterN0 = "IP:52.188.16.233:28000";
$Server::XLMasterN1 = "IP:198.50.214.196:28000";
$Server::XLMasterN2 = "IP:67.48.218.72:28000";
$Server::XLMasterN3 = "IP:72.54.15.185:28000";
$Server::XLMasterN4 = "IP:72.54.15.111:28000";
$Server::nummasters = "5";

$pref::PacketFrame = 32;
$pref::PacketRate = 50;
$pref::PacketSize = 500;
$TelnetPort="28012";
$TelnetPassword="578744";

$Server::MinVotes = "0";
$Server::MinVotesPct = "0.5";
$Server::MinVoteTime = "45";

$Server::respawnTime = "2";
$Server::TeamDamageScale = "1";
$Server::teamName0 = "Blood Eagle";
$Server::teamName1 = "Diamond Sword";
$Server::teamName2 = "Children of the Phoenix";
$Server::teamName3 = "Starwolf";
$Server::teamName4 = "Generic 1";
$Server::teamName5 = "Generic 2";
$Server::teamName6 = "Generic 3";
$Server::teamName7 = "Generic 4";
$Server::teamSkin0 = "beagle";
$Server::teamSkin1 = "dsword";
$Server::teamSkin2 = "cphoenix";
$Server::teamSkin3 = "swolf";
$Server::teamSkin4 = "base";
$Server::teamSkin5 = "base";
$Server::teamSkin6 = "base";
$Server::teamSkin7 = "base";
$Server::VoteAdminWinMargin = "1.1";
$Server::VoteFailTime = "30";
$Server::VoteWinMargin = "0.51";
$Server::VotingTime = "20";
$Server::warmupTime = "15";
$zadmin::pref::overflow::defaultpw = $Server::Password;