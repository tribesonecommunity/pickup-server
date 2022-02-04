// Define default to false for Training Mode.
    $PracticeMode::TrainingMode = false;

    // Serverside attachments.
    Attachment::AddAfter("displayMenuAdminMenu", "PracticeMode::displayMenuTrainingMenu");
    Attachment::AddAfter("processMenuAdminMenu", "PracticeMode::processMenuTrainingOptionsMenu");

    // add to player options menu
    function PracticeMode::displayMenuTrainingMenu(%cl) {
        addLine("Turn ON Training Mode", "trainingon", !$Server::TourneyMode && !$PracticeMode::TrainingMode, %cl);
        addLine("Turn OFF Training Mode", "trainingoff", !$Server::TourneyMode && $PracticeMode::TrainingMode, %cl);
    }

    function PracticeMode::processMenuTrainingOptionsMenu(%cl,%sel) {
        if (%sel == "trainingon") {
            $PracticeMode::TrainingMode = true;
            %cl.trainingmode = True;
            messageAll(0, "Training Mode has been ENABLED by an Admin.");
        }
        else if (%sel == "trainingoff") {
            $PracticeMode::TrainingMode = False;
            %cl.trainingmode = False;
            messageAll(1, "Training Mode has been DISABLED by an Admin.");
        }
        else { }
        
        Game::menuRequest(%cl);
    }

    // Serverside remotes for clients.
    function remoteStorePosition(%cl) {
        PracticeMode::storePlayerPosition(%cl);
    }
    function remoteRestorePosition(%cl) {
        PracticeMode::restorePlayerPosition(%cl);
    }
    function remoteRestoreHealth(%cl) {
        PracticeMode::restoreFullHealth(%cl);
    }

    // Store player data
    function PracticeMode::storePlayerPosition(%cl) {
        if ($PracticeMode::TrainingMode && !$Server::TourneyMode) {
            %clientId = %cl;
            %player = CLIENT::getOwnedObject(%cl);
            %clientId.position = GAMEBASE::getPosition(%cl);
            %clientId.rotation = GAMEBASE::getRotation(%cl);
            %clientId.velocity = ITEM::getVelocity(%cl);
            %clientId.health = GAMEBASE::getDamageLevel(%player);
            %clientId.energy = GAMEBASE::getEnergy(%player);
            $PracticeMode::storedPosition = true;
            PracticeMode::msgStoredPosition(%cl);
        }
        else {
            PracticeMode::disabledTraining(%cl);
        }
    }

    // Restore player data
    function PracticeMode::restorePlayerPosition(%cl) {
        
        if (!$PracticeMode::storedPosition) {
            PracticeMode::errorNotStored(%cl);
            return;
        }
        
        
        if ($PracticeMode::TrainingMode && !$Server::TourneyMode) {
            %player = CLIENT::getOwnedObject(%cl);
            GAMEBASE::setPosition(%cl, %cl.position);
            GAMEBASE::setRotation(%cl, %cl.rotation);
            ITEM::setVelocity(%cl, %cl.velocity);
            PracticeMode::restoreFullHealth(%cl);
            PracticeMode::restoreAmmo(%cl);
            GAMEBASE::setEnergy(%player, %cl.energy);
            PracticeMode::msgRestoredPosition(%cl);
        }
        else {
            PracticeMode::disabledTraining(%cl);
        }
    }

    // Restore player health
    function PracticeMode::restoreFullHealth(%cl) {
        if ($PracticeMode::TrainingMode && !$Server::TourneyMode) {
            %playerid = client::getOwnedObject(%cl);
            GAMEBASE::setDamageLevel(%playerid,0);
            PracticeMode::msgHealthRestored(%cl);
        }
        else {
            PracticeMode::disabledTraining(%cl);
        }
    }

    function PracticeMode::restoreAmmo(%cl) {
        if ($PracticeMode::TrainingMode && !$Server::TourneyMode) {
            Player::setItemCount(%cl,DiscLauncher,1);
            Player::setItemCount(%cl,GrenadeLauncher,1);
            Player::setItemCount(%cl,Chaingun,1);

            Player::setItemCount(%cl,DiscAmmo,200);
            Player::setItemCount(%cl,GrenadeAmmo,200);
            Player::setItemCount(%cl,BulletAmmo,200);

            Player::SetItemCount(%cl,Grenade,5);
            Player::SetItemCount(%cl,RepairKit,1);
        }
        else {
            PracticeMode::disabledTraining(%cl);
        }
    }

    // Client notifications from server.
    function PracticeMode::disabledTraining(%cl) {
        if (!$Server::TourneyMode) {
            Client::sendMessage(%cl, 1, "Training mode is disabled, please see the tab menu -> Player Options. ~wError_Message.wav");
        }
        else {
            Client::sendMessage(%cl, 1, "Training mode is disabled when in tournament mode. ~wError_Message.wav");
        }
    }

    function PracticeMode::errorNotStored(%cl) {
        Client::sendMessage(%cl, 1, "You must store a position first before you can restore. ~wError_Message.wav");
    }

    function PracticeMode::msgRestoredPosition(%cl) {
        Client::sendMessage(%cl, 1, "You have restored your player position. ~wfailpack.wav");
    }

    function PracticeMode::msgStoredPosition(%cl) {
        Client::sendMessage(%cl, 1, "You have stored your player position. ~wfailpack.wav");
    }
    
    function PracticeMode::msgHealthRestored(%cl) {
        Client::sendMessage(%cl, 1, "You have restored your health. ~wgenerator.wav");
    }