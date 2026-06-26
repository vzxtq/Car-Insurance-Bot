using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CarInsuranceBot.Bot.Ui;

public static class BotMessages
{
    public static string ProcessAlreadyStarted => "An insurance process is already in progress. Use /cancel first if you want to restart.";
    public static string Welcome => "Welcome to the Car Insurance Assistant. I will help you arrange car insurance quickly and clearly.";
    public static string DocumentsChecklist => "Please prepare a clear passport photo and a vehicle title photo with the VIN visible.";
    public static string AskPassport => "Upload a clear photo of the main page of your passport.";
    public static string AskVin => "Upload a photo of your vehicle title with the VIN clearly visible.";
    public static string StartRequired => "Please type /start to begin the insurance process.";
    public static string Canceled => "The insurance process has been cancelled. Type /start when you want to begin again.";
    public static string ValidImageRequired => "Please send a valid image file, such as JPG or PNG.";
    public static string ProcessingPassport => "Processing your passport image. Please wait.";
    public static string ProcessingVehicle => "Processing your vehicle title image. Please wait.";
    public static string UnexpectedDocument => "Unexpected document. Please follow the current instruction.";
    public static string PassportConfirmed => "Passport confirmed.";
    public static string PassportRejected => "Passport not confirmed. Please upload a new passport photo.";
    public static string VehicleConfirmed => "Vehicle title confirmed.";
    public static string VehicleRejected => "VIN not confirmed. Please upload a new vehicle title image with a clearly visible VIN.";
    public static string VinNotFound => "VIN was not found. Please upload a clearer vehicle title image.";
    public static string GeneratingPolicy => "Thank you. Generating your insurance policy.";
    public static string FinalDisagree => "Thank you for your time. Type /start if you decide to continue later.";
    public static string UnknownAction => "Unknown action. Please try again.";
    public static string MissingPassport => "Passport data is missing. Please type /cancel and restart the process.";
    public static string MissingVehicle => "Vehicle data is missing. Please type /cancel and restart the process.";

    public static string PassportConfirmation(string fullName, string documentNumber) => $"Name: {fullName}\nPassport: {documentNumber}\n\nIs this correct?";
    public static string VehicleConfirmation(string vin) => $"VIN: {vin}\n\nIs this correct?";
    public static string PriceOffer(string price) => $"The insurance cost is {price}. Would you like to proceed?";
    public static string FinalPriceOffer(string price) => $"The price is fixed at {price}. Would you like to continue with the policy?";
    public static string UnsupportedMessage(Message message) => message.Type == MessageType.Text ? StartRequired : "Please send text commands or the requested document image.";
}