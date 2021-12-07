//
// Set up the various player levels
// Add in order of Power, First added is no power
//
// addAdminLevel("Level Name");

addAdminLevel("Player");
addAdminLevel("Level 1 Admin");
addAdminLevel("Level 2 Admin");
addAdminLevel("Level 3 Admin");
addAdminLevel("Level 4 Admin");
addAdminLevel("Level 5 Admin");
 
// Define minimum level of adminship for each action below, by changing the accessLevel
// Each level also inherits the abilities of the level below it

$minAccessRequired::changeGameMode      = getAdminLevel("Level 1 Admin");
$minAccessRequired::changeMission     = getAdminLevel("Level 1 Admin");
$minAccessRequired::changePlyrTeam      = getAdminLevel("Player");
$minAccessRequired::EZConsoleShowsAsAdmin   = getAdminLevel("Level 1 Admin");
$minAccessRequired::forceMatchStart       = getAdminLevel("Level 1 Admin");
$minAccessRequired::kick          = getAdminLevel("Level 4 Admin");
$minAccessRequired::switchTeamDamage      = getAdminLevel("Level 1 Admin");

$minAccessRequired::announceTakeover        = getAdminLevel("Level 1 Admin");
$minAccessRequired::ban             = getAdminLevel("Level 5 Admin");
$minAccessRequired::cancelVote          = getAdminLevel("Level 1 Admin");
$minAccessRequired::changeTimeLimit       = getAdminLevel("Level 1 Admin");
$minAccessRequired::EZConsoleShowsAsSuperAdmin  = getAdminLevel("Level 1 Admin");
$minAccessRequired::makeAdmin         = getAdminLevel("Level 1 Admin");
$minAccessRequired::sendPrivateMsgs       = getAdminLevel("Level 1 Admin");
$minAccessRequired::setPassword             = getAdminLevel("Level 1 Admin");
$minAccessRequired::pickupMode              = getAdminLevel("Level 4 Admin");

$minAccessRequired::balancedMode              = getAdminLevel("Level 1 Admin");

$minAccessRequired::Mute            = getAdminLevel("Jesus Christ");
$minAccessRequired::receiveAlerts           = getAdminLevel("Level 1 Admin");
$minAccessRequired::seePlayerSpecs          = getAdminLevel("Level 1 Admin");
$minAccessRequired::sendWarning         = getAdminLevel("Level 1 Admin");

$minAccessRequired::permanentBan        = getAdminLevel("Level 5 Admin");
$minAccessRequired::resetServer         = getAdminLevel("Level 5 Admin");
$minAccessRequired::seePlayerlist       = getAdminLevel("Level 1 Admin");

$minAccessRequired::antiRape          = getAdminLevel("Level 4 Admin");
$minAccessRequired::antiRepair          = getAdminLevel("Level 4 Admin");
$minAccessRequired::stripAdmin          = getAdminLevel("Level 4 Admin");
$minAccessRequired::setTeamInfo         = getAdminLevel("Level 5 Admin");


$minAccessRequired::enableLasthope                = getAdminLevel("Level 5 Admin");

//add global spammers here to permanently remove global privileges
addGlobalSpammer("(c) Killa J");
addGlobalSpammerIP("127");