# Car Insurance Bot

Car Insurance Bot is a Telegram bot that guides a user through a simple car insurance purchase flow.

The bot starts a conversation, asks the user to upload a passport photo, extracts the holder name and document number, and asks the user to confirm the detected data. After that, it requests a vehicle title photo, reads the VIN, and asks for confirmation again.

Once the required data is confirmed, the bot shows the insurance price and asks whether the user wants to continue. If the user agrees, it generates an insurance policy from a text template and sends it back in the chat.

The project separates the main responsibilities into clear layers:

- Domain models describe the conversation state, passport data, vehicle data, and price.
- Application contracts define the bot's required capabilities without tying them to specific vendors.
- Infrastructure contains integrations for Telegram files, Gemini responses, Mindee document recognition, markdown escaping, and policy generation.
- Bot handlers process Telegram messages, uploaded files, and callback buttons.

Configuration values such as API keys, the VIN endpoint, the Gemini model, policy price, and template path are kept outside the business logic.