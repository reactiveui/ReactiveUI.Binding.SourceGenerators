### New Rules

 Rule ID     | Category | Severity | Notes                                                                                   
-------------|----------|----------|-----------------------------------------------------------------------------------------
 RXUIBIND001 | Usage    | Info     | Expression must be inline lambda for compile-time optimization                          
 RXUIBIND002 | Usage    | Warning  | Type has no observable properties                                                       
 RXUIBIND003 | Usage    | Warning  | Expression contains private/protected member                                            
 RXUIBIND004 | Usage    | Warning  | Type does not support before-change notifications                                       
 RXUIBIND005 | Usage    | Info     | Source type implements INotifyDataErrorInfo; validation binding requires runtime engine 
 RXUIBIND006 | Usage    | Warning  | Expression contains unsupported path segment (indexer, static field, read-only leaf field, or method call)           
 RXUIBIND007 | Usage    | Warning  | Control has no bindable event                                                           
 RXUIBIND008 | Usage    | Warning  | Property is not an IInteraction                                                         
 RXUIBIND009 | Usage    | Warning  | Generated binding dispatch is out of reach for this file
 RXUIBIND010 | Usage    | Warning  | Observed path passes through a type that raises no notification
 RXUIBIND011 | Usage    | Warning  | Binding call resolved to ReactiveUI's own mixin
 RXUIBIND012 | Usage    | Warning  | ToProperty source raises no notification generated code can reach
 RXUIBIND013 | Usage    | Warning  | ToProperty property must be named directly
 RXUIBIND014 | Usage    | Error    | Name the ToProperty initial value below C# 13
 RXUIBIND015 | Usage    | Warning  | Binding call names a type generated code cannot reach
