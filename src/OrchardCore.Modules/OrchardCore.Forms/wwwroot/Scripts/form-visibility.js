window.formVisibilityGroups=function(){let t={template:`
        <div class="mb-3">
            <!-- Loop through each group -->
            <div class="card mb-1" v-for="(group, groupIndex) in groups" :key="groupIndex">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span>Group {{ groupIndex + 1 }}</span>
                    <input type="hidden" :name="prefix + 'Groups[' + groupIndex + '].IsRemoved'" value="false" />
                    <button type="button" class="btn btn-sm btn-danger" @click="removeGroup(groupIndex)">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </div>
                        
                <div class="card-body">

                    <!-- Loop through each rule -->
                    <ul class="list-group w-100">
                        <!-- Loop through each rule in the group -->
                        <li class="list-group-item" v-for="(rule, ruleIndex) in group.rules" :key="ruleIndex">
                            <div class="row">
                                <div class="col">
                                    <select class="form-select" v-model="rule.field" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].Field'">
                                        <option value="">Select Field</option>
                                    <option v-for="option in filteredFieldOptions(rule.field)" :value="option.value">
                                        {{ option.text }}
                                        </option>
                                    </select>
                                </div>
                                <div class="col" :class="{'d-none': !rule.field}">
                                    <select class="form-select" v-model="rule.operator"
                                    :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].Operator'">
                                        <option value="">Select Operator</option>
                                        <option v-for="option in operatorsList(rule.field)" :value="option.value">
                                            {{ option.text }}
                                        </option>
                                    </select>
                                </div>
                                <div class="col" :class="{'d-none': !shouldShowValue(rule.operator)}">
                                    <input type="text" class="form-control" v-model="rule.value" placeholder="Value" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].Value'" />
                                </div>
                                <div class="col-auto">
                                    <input type="hidden" :name="prefix + 'Groups[' + groupIndex + '].Rules[' + ruleIndex + '].IsRemoved'" value="false" />
                                    <button type="button" class="btn btn-sm btn-danger" @click="removeRule(groupIndex, ruleIndex)">
                                        <i class="fa-solid fa-trash"></i>
                                    </button>
                                </div>
                            </div>
                        </li>
  
                    </ul>
                </div>

                <div class="card-footer">
                    <div class="d-flex justify-content-end">
                        <button type="button" class="btn btn-sm btn-primary" @click="addRule(groupIndex)">
                            <i class="fa-solid fa-plus"></i> New Rule
                        </button>
                    </div>
                </div>
            </div>
            <div class="d-flex justify-content-end p-3">
                <button type="button" class="btn btn-sm btn-primary" @click="addGroup()">
                    <i class="fa-solid fa-circle-plus"></i> New Group
                </button>
            </div>
        </div>
        `};return{initialize:e=>{let o=Object.assign({},t,e);if(!o.appElementSelector){console.error("appElementSelector is required");return}return new Vue({el:o.appElementSelector,data:()=>({groups:o.groupOptions||[],fieldOptions:o.FieldOptions||[],operatorOptions:o.operatorOptions||[],allOperatorOptions:o.operatorOptions||[],prefix:"",widgetId:o.widgetId,preloadedOptions:[]}),methods:{addGroup(){var t={rules:[{field:"",operator:"",value:""}]};this.groups.push(t),this.$set(this.groups,this.groups.length-1,t)},addRule(t){this.$set(this.groups[t].rules,this.groups[t].rules.length,{field:"",operator:"",value:""})},removeGroup(t){this.groups.splice(t,1)},removeRule(t,e){this.groups[t].rules.splice(e,1)},populateFields(){let t=this.getInputs(document);this.fieldOptions=t.map(function(t){return{value:t.htmlName,text:t.htmlName,type:t.htmlInputType}})},getInputs(t){let e=t.querySelectorAll(".widget-template"),o=[];return e.forEach(function(t){let e=t.querySelector('input[name$="FormInputElementPart.Name"]');if(e){let s=e.value.trim(),r="text",i=t.querySelector('select[name$="InputPart.Type"], select[name$="SelectPart.Editor"]');if(i&&(r=i.options[i.selectedIndex].value.toLowerCase()),!s||!r)return;o.push({htmlName:s,htmlInputType:r})}}),o},filteredFieldOptions(){let t=this.$el.closest(".widget-template");if(!t)return this.fieldOptions;let e=t.querySelector('input[name$="FormInputElementPart.Name"]')?.value.trim()||"";if(!e)return this.fieldOptions;let o=new Set;return this.fieldOptions.filter(t=>{let s=String(t.value||"").trim();return!(s===e||o.has(s))&&(o.add(s),!0)})},operatorsList(t){let e=this.fieldOptions.find(e=>e.value===t);if(!e)return[];let o=this.operatorMapping();return o[e.type]?this.allOperatorOptions.filter(t=>o[e.type].includes(t.value)):[]},operatorMapping:()=>({radio:["Is","IsNot","Empty","NotEmpty","Contains","DoesNotContain","StartsWith","EndsWith"],checkbox:["Is","IsNot","Contains","DoesNotContain"],text:["Is","IsNot","Empty","NotEmpty","Contains","DoesNotContain","StartsWith","EndsWith"],number:["Is","IsNot","GreaterThan","LessThan"],email:["Is","IsNot","Empty","NotEmpty"],tel:["Is","IsNot"],date:["Is","IsNot","GreaterThan","LessThan"],time:["Is","IsNot","GreaterThan","LessThan"],datetime:["Is","IsNot","GreaterThan","LessThan"],"datetime-local":["Is","IsNot","GreaterThan","LessThan"],month:["Is","IsNot"],week:["Is","IsNot"],hidden:["Is","IsNot"],password:["Is","IsNot","Empty","NotEmpty"],color:["Is","IsNot"],range:["Is","IsNot","GreaterThan","LessThan"],file:["Is","IsNot"],url:["Is","IsNot","Contains"],image:["Is","IsNot"],reset:["Is","IsNot"],search:["Is","IsNot","Contains"],dropdown:["Is","IsNot","Empty","NotEmpty","Contains","DoesNotContain","StartsWith","EndsWith"],textarea:["Is","IsNot","Empty","NotEmpty","Contains","DoesNotContain","StartsWith","EndsWith"],submit:[]}),toggleTabEvent(){document.addEventListener("shown.bs.tab",t=>{if(t.target.matches('[data-bs-toggle="tab"]')){var e=t.target.closest(".content-part-wrapper-form-part"),o=this.getInputs(e||document);this.fieldOptions=o.map(t=>({value:t.htmlName,text:t.htmlName,type:t.htmlInputType}))}})},shouldShowValue:t=>!!t&&"Empty"!==t&&"NotEmpty"!==t},mounted(){o.prefix&&(this.prefix=o.prefix+"."),this.toggleTabEvent(),this.groups=o.groupOptions||[],this.operatorOptions=o.operatorOptions||[],this.allOperatorOptions=o.operatorOptions||[],this.populateFields(),new MutationObserver(t=>{t.forEach(t=>{"childList"===t.type&&t.addedNodes.length&&(this.preloadedOptions=this.filteredFieldOptions())})}).observe(this.$el,{childList:!0,subtree:!0})},template:o.template})}}}();
//# sourceMappingURL=form-visibility.map