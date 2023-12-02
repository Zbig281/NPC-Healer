// Deactivate the LiFxNPCHealer package if active
deactivatePackage(LiFxNPCHealer);

// Create LiFxNPCHealer if it doesn't exist
if (!isObject(LiFxNPCHealer))
{
    new ScriptObject(LiFxNPCHealer)
    {
        triggers = new SimGroup(""); // Create a new SimGroup for triggers
        DisableDuringJH = false; // Set DisableDuringJH to false by default
    };
}

// Function to return the version of LiFxNPCHealer
function LiFxNPCHealer::version() {
    return "1.0.0";
}

// Function to set up callbacks for LiFxHealer
function LiFxHealer::setup() {
    // Register callbacks for LiFxHealer when JH starts or ends
    LiFxNPCHealer::registerCallback($LiFxNPCHealer::hooks::onJHStartCallbacks, disableHealer, LiFxHealer);
    LiFxNPCHealer::registerCallback($LiFxNPCHealer::hooks::onJHEndCallbacks, enableHealer, LiFxHealer);
}

// Function to enable healing after a delay
function LiFxNPCHealer::enableHealer() {
    // Enable the Heal() function after a delay of 30 minutes if DisableDuringJH is false and JH is not active
    if (LiFxNPCHealer.DisableDuringJH && !IsJHActive())
    {
        LiFxNPCHealer.schedule(30 * 60 * 1000, "Healsetup"); // 30 minutes delay in milliseconds
    }
}

// Function to perform healing
function LiFxNPCHealer::Heal(%client) {
    // Check if healing is disabled during JH and if JH is active
    if (LiFxNPCHealer.DisableDuringJH && IsJHActive())
    {
        LiFxNPCHealer::debugEcho("Do not heal during JH");
        // Send a message to the player informing them that healing is disabled during JH
        %client.cmSendClientMessage(2475, "I have been informed not to heal during JH");
        return;
    }
}

// Server command to handle LiFxNPCHealer functionality
function serverCmdLiFxNPCHealer(%client, %healingText) {
    echo("serverCmdLiFxNPCHealer called");

    // Set up callbacks for LiFxHealer
    LiFxHealer::setup();

    // Get player transform, save player state, and send a message to the client
    %transform = %client.player.getTransform();
    %client.player.savePlayer();
    %client.cmSendClientMessage(2475, "Your items have been repaired. ");

    // Update database entries for item durability, character HP, wounds, and effects
    dbi.Update("UPDATE items SET Durability = CreatedDurability WHERE ContainerID = (SELECT EquipmentContainerID FROM `character` WHERE ID =" SPC %client.charID SPC ") AND Durability < CreatedDurability");
    dbi.Update("UPDATE `character` SET HardHP = 2000000000, SoftHP = 2000000000 WHERE ID =" SPC %client.charID);
    dbi.Update("UPDATE `character_wounds` SET DurationLeft = 0 WHERE CharacterId = " SPC %client.charID);
    dbi.Update("DELETE FROM `character_effects` WHERE CharacterId =" SPC %client.charID);

    // Delete the player object, initialize the player manager after a short delay, and send a message to the client
    %client.player.delete();
    %client.schedule(100, "initPlayerManager");
    %client.player.client.cmSendClientMessage(2475, "Run along and get back into the fight!");
}

// Activate the LiFxNPCHealer package
activatePackage(LiFxNPCHealer);
