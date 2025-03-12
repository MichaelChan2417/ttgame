///> @todo: Replace hard code with config
export const CONFIG = {
    COLORS: ['#FF5252', '#4CAF50', '#2196F3', '#FFC107', '#9C27B0'],
    UI: {
      BUTTON: {
        START: {
          WIDTH_RATIO: 0.45,
          HEIGHT_RATIO: 0.12,
          COLOR: '#000000',
          IMG: './resources/yellow_button.png'
        },
        RETURN: {
          SIZE: 40,
          POSITION: { x: 40, y: 80 },
          IMG: './resources/return_button.png'
        }
      },
      BOARD: {
        SIZE: 6,
        PADDING: 10,
        BORDER: {
          COLOR: '#333',
          WIDTH: 2
        }
      },
      TIMER: {
        BG_IMG: './resources/time_background.png',
        FONT_SIZE_RATIO: 1/6
      }
    },
    GAME: {
      MAX_TIME: 1800
    }
};