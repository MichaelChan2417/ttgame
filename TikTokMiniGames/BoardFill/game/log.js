class Logger {
    constructor() {
      if (Logger._instance) return Logger._instance;
      Logger._instance = this;
      this._init();
    }
  
    _init() {
      this.logLevels = {
        INFO: 1,
        WARN: 2,
        ERROR: 3
      };
      this.currentLevel = this.logLevels.INFO;
    }
  
    #getCallerInfo() {
      try {
        const stack = new Error().stack.split('\n')[4];
        const match = /\((.*):(\d+):(\d+)\)$/.exec(stack);
        return {
          file: match?.[1]?.split('/').pop() || 'unknown',
          line: match?.[2] || '0'
        };
      } catch {
        return { file: 'unknown', line: '0' };
      }
    }
  
    #formatTime() {
      const now = new Date();
      return `${now.getHours().toString().padStart(2, '0')}:` +
             `${now.getMinutes().toString().padStart(2, '0')}:` +
             `${now.getSeconds().toString().padStart(2, '0')}.` +
             `${now.getMilliseconds().toString().padStart(3, '0')}`;
    }
  
    log(message, level = 'INFO') {
      const caller = this.#getCallerInfo();
      const timestamp = this.#formatTime();
      const logMessage = `[${timestamp}] [${level}] [${caller.file}:${caller.line}] ${message}`;
      
      // diff level
      switch(level) {
        case 'ERROR':
          console.error(logMessage);
          break;
        case 'WARN':
          console.warn(logMessage);
          break;
        default:
          console.log(logMessage);
      }
    }
  
    info(message) {
      this.log(message, 'INFO');
    }
  
    warn(message) {
      this.log(message, 'WARN');
    }
  
    error(message) {
      this.log(message, 'ERROR');
    }
}
  
export const logger = new Logger();