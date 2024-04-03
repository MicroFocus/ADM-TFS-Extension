/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2024 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using System.Collections.Generic;
using System.Xml.Serialization;
/**
 * The following schema fragment specifies the expected content contained within this class.
 * 
 * <complexType>
 *   <complexContent>
 *     <restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       <sequence>
 *         <element ref="{}skipped" minOccurs="0"/>
 *         <element ref="{}error" maxOccurs="unbounded" minOccurs="0"/>
 *         <element ref="{}failure" maxOccurs="unbounded" minOccurs="0"/>
 *         <element ref="{}system-out" maxOccurs="unbounded" minOccurs="0"/>
 *         <element ref="{}system-err" maxOccurs="unbounded" minOccurs="0"/>
 *       </sequence>
 *       <attribute name="name" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       <attribute name="assertions" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       <attribute name="time" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       <attribute name="classname" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       <attribute name="status" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       <attribute name="type" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       <attribute name="report" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     </restriction>
 *   </complexContent>
 * </complexType>
 * 
 */
namespace PSModule.AlmLabMgmtClient.Result.Model
{
    [XmlRoot(ElementName = "testcase")]
    public class TestCase
    {
        [XmlElement(ElementName = "skipped")]
        public string Skipped { get; set; }

        [XmlElement(ElementName = "error")]
        public List<Error> ListOfErrors { get; set; } = [];

        [XmlElement(ElementName = "failure")]
        public List<Failure> ListOfFailures { get; set; } = [];

        [XmlElement(ElementName = "system-out")]
        public List<string> ListOfSystemOuts { get; set; } = [];

        [XmlElement(ElementName = "system-err")]
        public List<string> ListOfSystemErrs { get; set; } = [];

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "assertions")]
        public string Assertions { get; set; }

        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }

        [XmlAttribute(AttributeName = "startExecDateTime")]
        public string StartExecDateTime { get; set; }

        [XmlAttribute(AttributeName = "classname")]
        public string Classname { get; set; }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "report")]
        public string Report { get; set; }
    }
}
