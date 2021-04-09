[<AutoOpen>]
module Jay.Exception

exception JsonParserException of msg: string with
    override this.Message = this.msg

exception PropertyNotFoundException of msg: string with
    override this.Message = this.msg

exception InvalidPropertyTypeException of msg: string with
    override this.Message = this.msg

exception InvalidCharacterException of msg: string with
    override this.Message = this.msg
